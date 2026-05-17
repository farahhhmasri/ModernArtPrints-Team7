using System.ComponentModel.DataAnnotations;

namespace ModernArtPrints.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public string? CurrentProfileImageUrl { get; set; }

        [Display(Name = "Change Profile Image")]
        public IFormFile? ProfileImage { get; set; }
    }
}