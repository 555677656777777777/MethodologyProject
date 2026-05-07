using CarDealershipAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarDealershipAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Offer> Offers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tell SQL Server exactly how to store decimal values
            modelBuilder.Entity<Car>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)"); // 18 digits total, 2 after decimal point

            modelBuilder.Entity<Offer>()
                .Property(o => o.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}