using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SuqiaWaterDistribution.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public int? RegionId { get; set; }
        public virtual Region? Region { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Driver? Driver { get; set; }
    }
}

