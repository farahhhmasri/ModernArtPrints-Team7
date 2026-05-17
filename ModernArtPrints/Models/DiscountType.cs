//namespace ModernArtPrints.Models
//{
//    public class DiscountType
//    {
//        public int Id { get; set; }

//        public string Name { get; set; } = string.Empty;

//        public string? Description { get; set; }

//        public List<Discount>? Discounts { get; set; }
//    }
//}


namespace ModernArtPrints.Models
{
    public enum DiscountType
    {
        Percentage,
        FixedAmount,
        FreeShipping,
        NoDiscount
    }
}