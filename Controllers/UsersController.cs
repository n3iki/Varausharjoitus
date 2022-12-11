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
        private readonly ReservationContext _context;
        private readonly IUserService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public UsersController(ReservationContext context, IUserService service, IUserAuthenticationService authenticationService)
        {
            _context = context;
            _service = service;
            _authenticationService = authenticationService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(long id, User user)
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
            try
            {
            _context.Entry(user).State = EntityState.Modified; //TÄSSÄ ONGELMA HUOM?
            }
            catch(Exception ex)
            {
                return Problem();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserDTO>> PostUser(User user)
        {
            //tarkistus, onko käyttäjällä oikeus muokata käyttäjää
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, user);

            if (!isAllowed) //jos ei
            {
                return Unauthorized();
            }

            UserDTO newUser = await _service.CreateUserAsync(user);
            if (newUser == null)
            {
                return Problem();
            }

            return CreatedAtAction("GetUser", new { firstName = newUser.FirstName }, newUser);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(long id)
        {


            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
