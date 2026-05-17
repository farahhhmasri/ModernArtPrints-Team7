namespace ModernArtPrints.Models.ViewModels
{

    public class CheckoutViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Apartment { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Jordan";
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountLabel { get; set; } = string.Empty;
        public bool HasFreeShipping { get; set; }

        // Stripe
        public string StripePublishableKey { get; set; } = string.Empty;
    }
}
