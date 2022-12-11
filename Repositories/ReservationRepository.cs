using Varausharjoitus.Models;
using Microsoft.EntityFrameworkCore;


namespace Varausharjoitus.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ReservationContext _context;

        public ReservationRepository(ReservationContext context) //konstruktori
        {
            _context = context;
        }

        public Task<Reservation> AddReservationAsync(Reservation reservation)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteReservationAsync(Reservation reservation)
        {
            throw new NotImplementedException();
        }

        public async Task<Reservation> GetReservationAsync(long id)
        {
            return await _context.Reservations.FindAsync(id);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsAsync()
        {
            return await _context.Reservations.ToListAsync();
        }

        public Task<Reservation> UpdateReservationAsync(Reservation reservation)
        {
            throw new NotImplementedException();
        }
    }
}
