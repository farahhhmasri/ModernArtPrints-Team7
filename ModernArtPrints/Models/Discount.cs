using System.ComponentModel.DataAnnotations;

namespace ModernArtPrints.Models
{
    public class Discount
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public decimal Value { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.NoDiscount;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}