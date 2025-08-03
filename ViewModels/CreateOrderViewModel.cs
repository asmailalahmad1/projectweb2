using System.ComponentModel.DataAnnotations;

namespace SuqiaWaterDistribution.ViewModels
{
    public class CreateOrderViewModel
    {
        public int TankId { get; set; }
        public string TankName { get; set; } = string.Empty;
        public decimal PricePerBarrel { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, 100, ErrorMessage = "الكمية يجب أن تكون بين 1 و 100")]
        [Display(Name = "الكمية (برميل)")]
        public int Quantity { get; set; } = 1;

        public decimal TotalPrice => Quantity * PricePerBarrel;
    }
}

