using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Varausharjoitus.Middleware;
using Varausharjoitus.Models;
using Varausharjoitus.Services;

namespace Varausharjoitus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public ItemsController (IItemService service, IUserAuthenticationService authenticationService)
        {
            _service = service;
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// GET kaikille itemeille
        /// </summary>
        /// <returns>list of items</returns>
        // GET: api/Items
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems()
        {
            return Ok(await _service.GetItemsAsync());
        }

        /// GET hakutermin perusteella
        // GET: api/Items/query
        [HttpGet("{query}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> QueryItems(string query)
        {
            return Ok(await _service.QueryItemsAsync(query));
        }

        /// GET tietyn käyttäjän itemeille
        // GET: api/Items/user/username
        [HttpGet("user/{username}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems(string username)
        {
            return Ok(await _service.GetItemsAsync(username));
        }

        /// <summary>
        /// GET yhdelle ID:lle
        /// </summary>
        /// <param name="id">Id of item</param>
        /// <returns>An item</returns>
        /// <response code="200">Returns the item</response>
        /// <response code="404">Item not found from database</response>
        // GET: api/Items/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ItemDTO>> GetItem(long id)
        {
            var item = await _service.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutItem(long id, ItemDTO item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            //tarkistus, onko käyttäjällä oikeus muokata itemiä
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);

            if(!isAllowed) //jos ei
            {
                return Unauthorized();
            }

            ItemDTO updatedItem = await _service.UpdateItemAsync(item);
            if(updatedItem == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST: api/Items
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ItemDTO>> PostItem(ItemDTO item)
        {
            //tarkistus, onko käyttäjällä oikeus muokata itemiä
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);

            if (!isAllowed) //jos ei
            {
                return Unauthorized();
            }

            ItemDTO newItem = await _service.CreateItemAsync(item);
            if (newItem == null)
            {
                return Problem();
            }

            return CreatedAtAction("GetItem", new { id = newItem.Id }, newItem);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(long id)

        {
            //tarkista oikeus
            ItemDTO item = new ItemDTO();
            item.Id = id;
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);

            if (await _service.DeleteItemAsync(id))
            {
                return Ok();
            }
            return NotFound();

            
        }


    }
}
