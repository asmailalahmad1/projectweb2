using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuqiaWaterDistribution.Models;

namespace SuqiaWaterDistribution.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Tank> Tanks { get; set; }
        public DbSet<TankRegion> TankRegions { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure TankRegion many-to-many relationship
            builder.Entity<TankRegion>()
                .HasKey(tr => new { tr.TankId, tr.RegionId });

            builder.Entity<TankRegion>()
                .HasOne(tr => tr.Tank)
                .WithMany(t => t.TankRegions)
                .HasForeignKey(tr => tr.TankId);

            builder.Entity<TankRegion>()
                .HasOne(tr => tr.Region)
                .WithMany(r => r.TankRegions)
                .HasForeignKey(tr => tr.RegionId);

            // Configure Customer relationship
            builder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId);

            // Configure Driver relationship
            builder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithOne(u => u.Driver)
                .HasForeignKey<Driver>(d => d.UserId);

            // Configure Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            builder.Entity<Order>()
                .HasOne(o => o.Tank)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TankId);

            builder.Entity<Order>()
                .HasOne(o => o.Driver)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DriverId)
                .IsRequired(false);

            // Seed data
            builder.Entity<Region>().HasData(
                new Region { Id = 1, Name = "البارة" },
                new Region { Id = 2, Name = "كنصفرة" },
                new Region { Id = 3, Name = "الفطيرة" },
                new Region { Id = 4, Name = "معرة النعمان" },
                new Region { Id = 5, Name = "سراقب" }
            );

            builder.Entity<Tank>().HasData(
                new Tank { Id = 1, Name = "خزان الشفا", Capacity = 1000, WaterType = "مياه شرب", PricePerBarrel = 50.00m, Location = "البارة" },
                new Tank { Id = 2, Name = "خزان النور", Capacity = 800, WaterType = "مياه شرب", PricePerBarrel = 45.00m, Location = "كنصفرة" },
                new Tank { Id = 3, Name = "خزان الحياة", Capacity = 1200, WaterType = "مياه شرب", PricePerBarrel = 55.00m, Location = "الفطيرة" }
            );

            builder.Entity<TankRegion>().HasData(
                new TankRegion { TankId = 1, RegionId = 1 },
                new TankRegion { TankId = 1, RegionId = 2 },
                new TankRegion { TankId = 2, RegionId = 2 },
                new TankRegion { TankId = 2, RegionId = 3 },
                new TankRegion { TankId = 3, RegionId = 3 },
                new TankRegion { TankId = 3, RegionId = 4 }
            );
        }
    }
}

