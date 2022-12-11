using Varausharjoitus.Models;

namespace Varausharjoitus.Repositories
{
    public interface IReservationRepository
    {
        public Task<Reservation> GetReservationAsync(long id); //get yhdelle varaukselle
        public Task<IEnumerable<Reservation>> GetReservationsAsync(); //get kaikille varauksille
        public Task<Reservation> AddReservationAsync(Reservation reservation);
        public Task<Reservation> UpdateReservationAsync(Reservation reservation);
        public Task<Boolean> DeleteReservationAsync(Reservation reservation);
    }
}
