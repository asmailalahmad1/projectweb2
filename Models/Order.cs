using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuqiaWaterDistribution.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int TankId { get; set; }
        public virtual Tank Tank { get; set; } = null!;

        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime OrderTime { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "New"; // New, Accepted, InDelivery, Delivered, Rejected, Cancelled

        public int? Rating { get; set; } // 1-5 stars
        public string? Comment { get; set; }
    }
}

