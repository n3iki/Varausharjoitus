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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public UsersController(IUserService service, IUserAuthenticationService authenticationService)
        {
            _service = service;
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Palauttaa kaikki käyttäjät
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /users/
        /// </remarks>
        [HttpGet]
        [ActionName(nameof(GetUser))] //sisäinen routing menee sekaisin Async -loppuisesta funktiosta, tämä estää userin postin errorit
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return Ok(await _service.GetUsersAsync());
        }

        /// <summary>
        /// Palauttaa haetun käyttäjän ID:n perusteella
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /users/käyttäjän id
        /// </remarks>
        [HttpGet("{id}")]

        public async Task<ActionResult<UserDTO>> GetUser(long id)
        {
            var user = await _service.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Muokkaa käyttäjää
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     POST /users/käyttäjän id
        ///     {
        ///      "id": käyttjän id,
        ///      "userName": "Käyttäjätunnus",
        ///      "password": "salasana",
        ///      "firstName": "Etunimi",
        ///      "lastName": "Sukunimi"
        ///     }
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(long id, UserDTO user)
        {
            if (id != user.Id) //tarkistetaan löytyykö käyttäjää
            {
                return BadRequest();
            }

            //tarkistus, onko käyttäjällä oikeus muokata useria
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, user);

            if (!isAllowed) //jos ei
            {
                return Unauthorized();
            }

            UserDTO updatedUser = await _service.UpdateUserAsync(user);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Lisää uuden käyttäjän
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     POST /users
        ///     {
        ///      "userName": "Käyttäjätunnus",
        ///      "password": "salasana",
        ///      "firstName": "Etunimi",
        ///      "lastName": "Sukunimi"
        ///     }
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(User user)
        {
         

            UserDTO newUser = await _service.CreateUserAsync(user);
            if (newUser == null)
            {
                return Problem();
            }

            return CreatedAtAction((nameof(GetUser)), new { firstName = newUser.FirstName }, newUser);
        }

        /// <summary>
        /// Poistaa käyttäjän
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     DELETE /users/käyttäjän id
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(long id)
        {
            //tarkista oikeus
            UserDTO user = new UserDTO();
            user.Id = id;
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, user);

            if (await _service.DeleteUserAsync(id))
            {
                return Ok();
            }
            return NotFound();


        }

    }
}
