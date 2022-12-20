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
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationContext _context;
        private readonly IReservationService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public ReservationsController(ReservationContext context, IReservationService service, IUserAuthenticationService authenticationService)
        {
            _context = context;
            _service = service;
            _authenticationService = authenticationService;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return Ok(await _service.GetReservationsAsync());
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDTO>> GetReservation(long id)
        {
            var reservation = await _service.GetReservationAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // PUT: api/Reservations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutReservation(long id, ReservationDTO reservation)
        {
            if (id != reservation.Id)
            {
                return BadRequest();
            }

            //tarkistus, onko käyttäjällä oikeus muokata reservationia
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, reservation);

            if (!isAllowed) //jos ei
            {
                return Unauthorized();
            }

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
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

        // POST: api/Reservations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReservationDTO>> PostReservation(ReservationDTO reservation)
        {

            //tarkistus, onko käyttäjällä oikeus muokata reservationia
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, reservation);

            if (!isAllowed) //jos ei
            {
                return Unauthorized();
            }

            reservation = await _service.CreateReservationAsync(reservation);
            if (reservation == null)
            {
                return Problem();
            }

            return CreatedAtAction("GetReservation", new { id = reservation.Id }, reservation);
        
     
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReservation(long id)
        {

            //tarkista oikeus
            ReservationDTO reservation = new ReservationDTO();
            reservation.Id = id;
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, reservation);

            if (await _service.DeleteReservationAsync(id))
            {
                return Ok();
            }
            return NotFound();

            
        }

        private bool ReservationExists(long id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
