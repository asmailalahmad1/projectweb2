using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuqiaWaterDistribution.Models
{
    public class Tank
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Required]
        [StringLength(50)]
        public string WaterType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerBarrel { get; set; }

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<TankRegion> TankRegions { get; set; } = new List<TankRegion>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

