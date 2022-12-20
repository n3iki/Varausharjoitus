using Varausharjoitus.Models;
using Varausharjoitus.Repositories;

namespace Varausharjoitus.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IItemRepository _itemRepository;

        public ReservationService(IReservationRepository repository, IUserRepository userRepository, IItemRepository itemRepository) //konstruktori
        {
            _repository = repository;
            _userRepository = userRepository;
            _itemRepository = itemRepository;
        }

        public async Task<ReservationDTO> CreateReservationAsync(ReservationDTO dto)
        {
            if(dto.StartTime >= dto.EndTime) //jos alkuaika on sama tai myöhempi kuin loppuaika
            {
                return null;
            }

            Item target = await _itemRepository.GetItemAsync(dto.Target); //tarkistetaan että itemi on olemassa
            if(target == null)
            {
                return null;
            }

            IEnumerable<Reservation> reservations = await _repository.GetReservationsAsync(target, dto.StartTime, dto.EndTime); //tarkistetaan onko itemille jo olemassaoleva varraus tälle aikavälille
            if(reservations.Count() > 0)
            {
                return null;
            }

            Reservation newReservation = await DTOToReservation(dto);
            newReservation = await _repository.AddReservationAsync(newReservation);
            return ReservationToDTO(newReservation);
        }

        public async Task<bool> DeleteReservationAsync(long id)
        {
            Reservation oldReservation = await _repository.GetReservationAsync(id);
            if (oldReservation == null)
            {
                return false;
            }
            return await _repository.DeleteReservationAsync(oldReservation);
        }

        public async Task<ReservationDTO> GetReservationAsync(long id)
        {
            Reservation reservation = await _repository.GetReservationAsync(id);

            if (reservation != null)
            {
                return ReservationToDTO(reservation);
            }
            return null;
        }

        public async Task<IEnumerable<ReservationDTO>> GetReservationsAsync()
        {
            IEnumerable<Reservation> reservations = await _repository.GetReservationsAsync();
            List<ReservationDTO> result = new List<ReservationDTO>();
            foreach (Reservation r in reservations)
            {
                result.Add(ReservationToDTO(r));
            }
            return result;
        }

        public async Task<ReservationDTO> UpdateReservationAsync(ReservationDTO reservation)
        {
            Reservation oldReservation = await _repository.GetReservationAsync(reservation.Id);
            if (oldReservation == null)
            {
                return null;
            }

            //Hae omistaja kannasta
            User owner = await _userRepository.GetUserAsync(reservation.Owner);
            if (owner != null)
            {
                oldReservation.Owner = owner;
            }

            //hae varauksen kohde kannasta
            Item item = await _itemRepository.GetItemAsync(reservation.Target);
            if (item != null)
            {
                oldReservation.Target = item;
            }

            oldReservation.StartTime = reservation.StartTime;
            oldReservation.EndTime = reservation.EndTime;
            Reservation updatedReservation = await _repository.UpdateReservationAsync(oldReservation);
            if (updatedReservation == null)
            {
                return null;
            }
            return ReservationToDTO(updatedReservation);
        }


        /// /// /// /// /// ///
        /// MUUTOSFUNKTIOT: ///
        /// /// /// /// /// ///

        private async Task<Reservation> DTOToReservation(ReservationDTO dto)
        {

            Reservation newReservation = new Reservation();
            newReservation.StartTime = dto.StartTime;
            newReservation.EndTime = dto.EndTime;

            //Hae omistaja kannasta
            User owner = await _userRepository.GetUserAsync(dto.Owner);
            if (owner != null)
            {
                newReservation.Owner = owner;
            }

            //hae kohde kannasta
            Item target = await _itemRepository.GetItemAsync(dto.Target);
            if (target != null)
            {
                newReservation.Target = target;
            }

            return newReservation;
        }
        private ReservationDTO ReservationToDTO(Reservation reservation)
        {
            ReservationDTO dto = new ReservationDTO();
            dto.StartTime = reservation.StartTime;
            dto.EndTime = reservation.EndTime;
            if (reservation.Owner != null)
            {
                dto.Owner = reservation.Owner.UserName;

            }
            if (reservation.Target != null)
            {
                dto.Target = reservation.Target.Id;
            }

            return dto;

        }
    }
}
