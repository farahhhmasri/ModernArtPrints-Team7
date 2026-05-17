using System.ComponentModel.DataAnnotations;

namespace ModernArtPrints.Models.ViewModels
{
    public class OrderReviewViewModel
    {
        public int OrderId { get; set; }

        public List<ProductReviewItemViewModel> Products { get; set; } = new List<ProductReviewItemViewModel>();
    }

    public class ProductReviewItemViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        public string? CommentText { get; set; }
    }
}