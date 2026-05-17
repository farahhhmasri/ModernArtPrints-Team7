using System.ComponentModel.DataAnnotations;

namespace ModernArtPrints.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        public string CommentText { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}