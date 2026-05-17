namespace ModernArtPrints.Models.ViewModels
{
    public class ShopViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "rating"

        public int CartCount { get; set; }
    }
}


