using System.ComponentModel.DataAnnotations;

namespace ModernArtPrints.Areas.Admin.Models
{
    public class CategoryCreateViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
