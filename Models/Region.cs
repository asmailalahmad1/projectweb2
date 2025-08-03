using System.ComponentModel.DataAnnotations;

namespace SuqiaWaterDistribution.Models
{
    public class Region
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
        public virtual ICollection<TankRegion> TankRegions { get; set; } = new List<TankRegion>();
    }
}

