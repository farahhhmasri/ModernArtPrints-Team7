namespace ModernArtPrints.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Testimonial> Testimonials { get; set; }
        public List<Product> SaleProducts { get; set; }
        public List<Category> Categories { get; set; } = new();
    }
}
