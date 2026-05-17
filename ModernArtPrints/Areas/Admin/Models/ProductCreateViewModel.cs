using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace ModernArtPrints.Areas.Admin.Models
{
    public class ProductCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }

        public int? DiscountId { get; set; }

        public IFormFile? MainImageFile { get; set; }

        public List<IFormFile>? AdditionalImages { get; set; }
    }
}
