namespace ModernArtPrints.Areas.Admin.Models
{
        public class UserDetailsViewModel
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
            public bool EmailConfirmed { get; set; }
            public string ProfileImageUrl { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<string> Roles { get; set; } = new();
            public bool IsSuspended { get; set; }
        }
    }

