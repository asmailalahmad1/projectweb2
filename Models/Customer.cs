using System.ComponentModel.DataAnnotations;

namespace SuqiaWaterDistribution.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public int RegionId { get; set; }
        public virtual Region Region { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

