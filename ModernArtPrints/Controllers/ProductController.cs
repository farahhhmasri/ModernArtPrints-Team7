using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using ModernArtPrints.Models.ViewModels;

namespace ModernArtPrints.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Shop(int? categoryId, string? search, string? sortBy)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductReviews)
                .Include(p => p.Discount)
                .AsQueryable();

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search) || p.Category.Name.Contains(search));

            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "rating" => products.OrderByDescending(p => p.ProductReviews.Average(r => r.Rating)),
                _ => products
            };

            // Get cart count
            int cartCount = 0;
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                cartCount = await _context.CartItems
                    .Where(c => c.Cart.UserId == userId)
                    .SumAsync(c => c.Quantity);
            }
            else
            {
                // Guest cart is stored in DB under a SessionId
                var sessionId = HttpContext.Session.GetString("CartSessionId");
                if (!string.IsNullOrEmpty(sessionId))
                {
                    cartCount = await _context.CartItems
                        .Where(c => c.Cart.SessionId == sessionId)
                        .SumAsync(c => c.Quantity);
                }
            }

            var shopView = new ShopViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Products = await products.ToListAsync(),
                SelectedCategoryId = categoryId,
                SearchQuery = search,
                SortBy = sortBy,
                CartCount = cartCount
            };

            return View(shopView);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Get cart count for the icon
            int cartCount = 0;
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                cartCount = await _context.CartItems
                    .Where(c => c.Cart.UserId == userId)
                    .SumAsync(c => c.Quantity);
            }
            else
            {
                var sessionId = HttpContext.Session.GetString("CartSessionId");
                if (!string.IsNullOrEmpty(sessionId))
                {
                    cartCount = await _context.CartItems
                        .Where(c => c.Cart.SessionId == sessionId)
                        .SumAsync(c => c.Quantity);
                }
            }

            ViewBag.CartCount = cartCount;

            return View(product);
        }
    }
}