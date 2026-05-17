using System.ComponentModel.DataAnnotations.Schema;

namespace ModernArtPrints.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        public int? DiscountId { get; set; }

        public Discount? Discount { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.processing;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string? ShippingAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public List<OrderItem>? OrderItems { get; set; }

        public Invoice? Invoice { get; set; }

        public Payment? Payment { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}