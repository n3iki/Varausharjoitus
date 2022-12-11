using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using Varausharjoitus.Models;

namespace Varausharjoitus.Middleware
{
    public interface IUserAuthenticationService
    {
        Task<User> Authenticate(string username, string password);
        Task<bool> IsAllowed(String username, ItemDTO item); //tarkistetaan saako käyttäjä muokata itemiä
        Task<bool> IsAllowed(String username, User user); //tarkistetaan saako käyttäjä muokata useria
        Task<bool> IsAllowed(String username, ReservationDTO reservation); //tarkistetaan saako käyttäjä muokata reservationia
    }

    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly ReservationContext _context;

        public UserAuthenticationService(ReservationContext context)
        {
            _context = context;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            User? user = await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync(); //etsitään käyttäjä käyttäjänimen perusteella

            if(user == null) // jos käyttäjää ei löydy
            {
                return null;
            }
            //lisätään käyttäjälle suola ja salasanan hash:
            byte[] salt = user.Salt;
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if(hashedPassword != user.Password)
            {
                return null;
            }


            return user;
        }

        public async Task<bool> IsAllowed(string username, ItemDTO item)  //tarkistetaan saako käyttäjä muokata itemiä tai postata uutta
        {
            User? user = await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync(); //etsitään käyttäjä käyttäjänimen perusteella
            Item? dbItem = await _context.Items.Include(i => i.Owner).FirstOrDefaultAsync(i => i.Id == item.Id); //tarkistetaan että item kuuluu käyttäjälle

            if(user==null) //jos käyttäjää ei löydy
            {
                return false;
            }
            if(dbItem == null && item.Owner == user.UserName) //jos lisätään uutta itemiä eli itemiä ei löydy tietokannasta, tarkistetaan että postattavan itemin omistaja on sama kuin postaaja
            {
                return true;
            }
            if (dbItem == null) //mikäli itemiä ei löytynyt
            {
                return false;
            }
            if(user.Id == dbItem.Owner.Id) //jos käyttäjä ja itemin omistaja natsaa kun halutaan editoida itemiä
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsAllowed(string username, User user) //tarkistetaan saako käyttäjä muokata useria
        {
            User? dbuser = await _context.Users.Where(x => x.UserName == user.UserName).FirstOrDefaultAsync(); //etsitään käyttäjänimi käyttäjistä

            if (dbuser.UserName == username) //tarkistetaan onko muokkaava käyttäjä sama kuin muokattava käyttäjä
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsAllowed(string username, ReservationDTO reservation) //tarkistetaan saako käyttäjä muokata reservationia
        {
            User? user = await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync(); //etsitään käyttäjä käyttäjänimen perusteella
            Reservation? dbReservation = await _context.Reservations.Include(i => i.Owner).FirstOrDefaultAsync(i => i.Id == reservation.Id); //tarkistetaan että käyttäjä yrittää muokata omaa varaustaan


            if (user == null) //jos käyttäjää ei löydy
            {
                return false;
            }


            if (dbReservation.Id == null && dbReservation.Owner.UserName == user.UserName) //jos lisätään uutta varausta eli varausta ei löydy tietokannasta, tarkistetaan että postattavan varauksen omistaja on sama kuin postaaja
            {
                return true;
            }

            if (dbReservation == null) //mikäli varausta ei löytynyt
            {
                return false;
            }

            if (user.Id == dbReservation.Id) //jos käyttäjä ja itemin reservation natsaa
            {
                return true;
            }
            return false;
        }
    }
}
