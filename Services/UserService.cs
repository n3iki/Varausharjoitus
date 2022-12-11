using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NuGet.Configuration;
using System.Security.Cryptography;
using Varausharjoitus.Models;
using Varausharjoitus.Repositories;

namespace Varausharjoitus.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepository _repository;


        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserDTO> CreateUserAsync(User user)
        {
            byte[] salt = new byte[128 / 8]; //suola käyttäjän salasanaan

            using (var rng = RandomNumberGenerator.Create()) //satunnainen suola jokaiselle käyttäjälle
            {
                rng.GetBytes(salt);
            }
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: user.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            User newUser = new User // käyttäjä, jolla hashattu salasana
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Salt = salt,
                Password = hashedPassword,
                CreatedDate = DateTime.Now
            };


            newUser = await _repository.AddUserAsync(newUser);
            if(newUser == null)
            {
                return null;
            }
            return UserToDTO(newUser);
        }


        public async Task<bool> DeleteUserAsync(string UserName)
        {
            User oldUser = await _repository.GetUserAsync(UserName);
            if (oldUser == null)
            {
                return false;
            }

            return await _repository.DeleteUserAsync(oldUser);
        }

        public async Task<UserDTO> GetUserAsync(string UserName)
        {
            User user = await _repository.GetUserAsync(UserName);

            if (user != null)
            {
                await _repository.UpdateUserAsync(user);
                return UserToDTO(user);
            }
            return null;
        }

        public async Task<IEnumerable<UserDTO>> GetUsersAsync()
        {
            IEnumerable<User> users = await _repository.GetUsersAsync();
            List<UserDTO> result = new List<UserDTO>();
            foreach (User i in users)
            {
                result.Add(UserToDTO(i));
            }
            return result;
        }

        public async Task<UserDTO> UpdateUserAsync(UserDTO user)
        {
            User oldUser = await _repository.GetUserAsync(user.UserName);
            if (oldUser == null)
            {
                return null;
            }

            oldUser.FirstName = user.FirstName;
            oldUser.LastName = user.LastName;

            User updatedUser = await _repository.UpdateUserAsync(oldUser);
            if (updatedUser == null)
            {
                return null;
            }
            return UserToDTO(updatedUser);
        }

        private UserDTO UserToDTO(User user)
        {
            UserDTO dto = new UserDTO();
            dto.FirstName = user.FirstName;
            dto.LastName = user.LastName;
            dto.UserName = user.UserName;
            dto.CreatedDate = user.CreatedDate;
            dto.LoginDate = user.CreatedDate;
            
            return dto;
        }

        private async Task<User> DTOToUser(UserDTO dto)
        {
            User user = new User();
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.UserName = dto.UserName;
            user.CreatedDate = dto.CreatedDate;
            user.LoginDate = dto.LoginDate;

            return user;
        }
    }
}
