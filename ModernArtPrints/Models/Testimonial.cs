using System.ComponentModel.DataAnnotations;

namespace ModernArtPrints.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}