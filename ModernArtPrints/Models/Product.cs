using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModernArtPrints.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }

        [Required]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        public string? MainImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<ProductImage>? ProductImages { get; set; } = new List<ProductImage>();

        public ICollection<Wishlist>? Wishlists { get; set; } = new List<Wishlist>();

        public ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<ProductReview>? ProductReviews { get; set; } = new List<ProductReview>();

        public ICollection<CartItem>? CartItems { get; set; } = new List<CartItem>();
    }
}