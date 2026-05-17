using System.ComponentModel.DataAnnotations.Schema;

namespace ModernArtPrints.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public Guid InvoiceNumber { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public DateTime IssuedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}