using Microsoft.EntityFrameworkCore;

namespace Varausharjoitus.Models
{
    public class ReservationContext : DbContext
    {
        public ReservationContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}
