using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]
   
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? searchString, string? role)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u =>
                    u.UserName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.FullName.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;
            ViewData["Role"] = role;

            var userList = await users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    Address = user.Address,
                    ProfileImageUrl = user.ProfileImageUrl,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                });
            }

            if (!string.IsNullOrEmpty(role))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(role)).ToList();
            }

            return View(userViewModels);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                Orders = orders
            };

            return View(viewModel);
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserDetailsViewModel : UserViewModel
    {
        public string? PhoneNumber { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}