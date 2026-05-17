namespace ModernArtPrints.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // للمستخدم المسجل دخول
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // للزائر غير المسجل
        public string? SessionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}