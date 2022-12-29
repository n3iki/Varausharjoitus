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
        /// Palauttaa kaikki itemit
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /items
        ///</remarks>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems()
        {
            return Ok(await _service.GetItemsAsync());
        }


        /// <summary>
        /// Palauttaa itemit, jonka nimessä hakusana
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /items/hakusana
        ///</remarks>
        [HttpGet("{query}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> QueryItems(string query)
        {
            return Ok(await _service.QueryItemsAsync(query));
        }


        /// <summary>
        /// Palauttaa yhden käyttäjän kaikki itemit
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /items/user/käyttäjän id
        ///</remarks>
        [HttpGet("user/{id:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems(long id)
        {
            return Ok(await _service.GetItemsAsync(id));
        }


        /// <summary>
        /// Palauttaa haetun itemin ID:n perusteella
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /items/itemin id
        ///</remarks>
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


        /// <summary>
        /// Muokkaa itemiä
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     PUT /items/id
        ///     {
        ///     "name": "Itemin nimi",
        ///     "description": "Itemin lisätiedot",
        ///     "owner": omistajan ID,
        ///     "images": [
        ///       {
        ///         "url": "kuvaosoite",
        ///         "description": "kuvateksti"
        ///       }
        ///               ]
        ///       }
        ///</remarks>
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


        /// <summary>
        /// Lisää uuden itemin
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     POST /items
        ///     {
        ///     "name": "Itemin nimi",
        ///     "description": "Itemin lisätiedot",
        ///     "owner": omistajan ID,
        ///     "images": [
        ///       {
        ///         "url": "kuvaosoite",
        ///         "description": "kuvateksti"
        ///       }
        ///               ]
        ///       }
        ///</remarks>
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


        /// <summary>
        /// Poistaa itemin
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     DELETE /items/itemin id
        ///</remarks>
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
