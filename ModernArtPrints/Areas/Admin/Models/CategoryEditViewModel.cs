namespace ModernArtPrints.Areas.Admin.Models
{
    public class CategoryEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ExistingImageUrl { get; set; }
        public IFormFile? NewImageFile { get; set; }
    }
}
