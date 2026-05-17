using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;

namespace ModernArtPrints.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Wishlist Page
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.Category)
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductReviews)
                .Include(w => w.Product)
                    .ThenInclude(p => p.Discount)
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.Id)
                .ToListAsync();

            // Get cart count for the icon
            var cartCount = await _context.CartItems
                .Where(c => c.Cart.UserId == user.Id)
                .SumAsync(c => c.Quantity);

            ViewBag.CartCount = cartCount;

            return View(wishlistItems);
        }

        // Add To Wishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
                return NotFound();

            bool alreadyExists = await _context.Wishlists
                .AnyAsync(w => w.UserId == user.Id && w.ProductId == productId);

            if (!alreadyExists)
            {
                _context.Wishlists.Add(new Wishlist
                {
                    UserId = user.Id,
                    ProductId = productId
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "Item added to wishlist.";
            }
            else
            {
                TempData["Info"] = "Item already exists in wishlist.";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // Remove From Wishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int wishlistId)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == user.Id);

            if (wishlistItem == null)
                return NotFound();

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Item removed from wishlist.";
            return RedirectToAction(nameof(Index));
        }

        // Add to Cart AND Remove from Wishlist in one action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartAndRemove(int wishlistId, int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            // --- Add to Cart ---
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound();

            // Get or create cart
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                if (existingItem.Quantity > product.StockQuantity)
                    existingItem.Quantity = product.StockQuantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price
                });
            }

            // --- Remove from Wishlist ---
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == user.Id);

            if (wishlistItem != null)
                _context.Wishlists.Remove(wishlistItem);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Item added to cart.";

            // Stay on wishlist page
            return RedirectToAction(nameof(Index));
        }
    }
}