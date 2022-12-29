using Varausharjoitus.Models;

namespace Varausharjoitus.Services
{
    public interface IUserService
    {
        public Task<UserDTO> CreateUserAsync(User user);
        public Task<UserDTO> GetUserAsync(long id);
        public Task<IEnumerable<UserDTO>> GetUsersAsync();
        public Task<UserDTO> UpdateUserAsync(UserDTO user);
        public Task<Boolean> DeleteUserAsync(long id);
    }
}
