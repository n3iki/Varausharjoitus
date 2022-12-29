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
        private readonly IReservationService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public ReservationsController(IReservationService service, IUserAuthenticationService authenticationService)
        {
            _service = service;
            _authenticationService = authenticationService;
        }


        /// <summary>
        /// Palauttaa kaikki varaukset
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /reservations/
        ///</remarks>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return Ok(await _service.GetReservationsAsync());
        }

        /// <summary>
        /// Palauttaa haetun varauksen ID:n perusteella
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     GET /reservations/varauksen id
        ///</remarks>
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

        /// <summary>
        /// Muokkaa varausta
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     PUT /reservations
        ///     {
        ///      "target": itemin ID,
        ///      "owner": käyttäjän ID,
        ///      "startTime": yyyy-MM-ddTHH:mm:ss.SSSz,
        ///      "startTime": yyyy-MM-ddTHH:mm:ss.SSSz,
        ///     }
        /// </remarks>
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

            ReservationDTO updatedReservation = await _service.UpdateReservationAsync(reservation);
            if (updatedReservation == null)
            {
                return NotFound();
            }
            return NoContent();
        }



        /// <summary>
        /// Lisää uuden varauksen
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     POST /reservations
        ///     {
        ///      "target": itemin ID,
        ///      "owner": käyttäjän ID,
        ///      "startTime": yyyy-MM-ddTHH:mm:ss.SSSz,
        ///      "startTime": yyyy-MM-ddTHH:mm:ss.SSSz,
        ///     }
        /// </remarks>
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

        /// <summary>
        /// Poistaa varauksen
        /// </summary>
        /// <remarks>
        /// Esimerkkipyyntö:
        /// 
        ///     DELETE /reservations/varauksen ID
        ///</remarks>
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

    }
}
