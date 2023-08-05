using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Models
{
    public class AssignmentContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Laptop>().HasKey(l => l.Number);
            modelBuilder.Entity<Store>().HasKey(s => s.StoreNumber);
            modelBuilder.Entity<Laptop>().Property(l => l.Price).HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Laptop>()
                .HasOne(l => l.Brand)
                .WithMany(b => b.Laptops)
                .HasForeignKey(l => l.BrandId);

            modelBuilder.Entity<LaptopStore>()
                .HasOne(ls => ls.Laptop)
                .WithMany(l => l.LaptopStores)
                .HasForeignKey(ls => ls.LaptopId);

            modelBuilder.Entity<LaptopStore>()
                .HasOne(ls => ls.Store)
                .WithMany(sl => sl.LaptopStores)
                .HasForeignKey(ls => ls.StoreNumber);

            modelBuilder.Entity<LaptopStore>().HasKey(ls => new { ls.LaptopId, ls.StoreNumber });

        }

        public AssignmentContext(DbContextOptions options) : base(options) { }

        public DbSet<Laptop> Laptops { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<LaptopStore> LaptopStores { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;

    }
}
