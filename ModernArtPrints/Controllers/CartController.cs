using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using System.Security.Claims;

namespace ModernArtPrints.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCart();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartId == cart.Id)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Product was not found."
                });
            }

            var cart = await GetOrCreateCart();

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.CartId == cart.Id &&
                    c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;

                if (existingItem.Quantity > product.StockQuantity)
                {
                    existingItem.Quantity = product.StockQuantity;
                }

                existingItem.TotalPrice =
                    existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * quantity
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.CartItems
                .Where(c => c.CartId == cart.Id)
                .SumAsync(c => c.Quantity);

            return Json(new
            {
                success = true,
                cartCount = cartCount,
                message = $"{product.Name} has been added to your cart."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseQuantity(int cartItemId)
        {
            var item = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (item != null &&
                item.Quantity < item.Product.StockQuantity)
            {
                item.Quantity++;

                item.TotalPrice =
                    item.Quantity * item.UnitPrice;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseQuantity(int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.TotalPrice =
                        item.Quantity * item.UnitPrice;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (item != null)
            {
                _context.CartItems.Remove(item);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        private async Task<Cart> GetOrCreateCart()
        {
            Cart? cart;

            if (User.Identity!.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId
                    };

                    _context.Carts.Add(cart);

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var sessionId = HttpContext.Session.GetString("CartSessionId");

                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString();

                    HttpContext.Session.SetString(
                        "CartSessionId",
                        sessionId);
                }

                cart = await _context.Carts
                    .FirstOrDefaultAsync(c =>
                        c.SessionId == sessionId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        SessionId = sessionId
                    };

                    _context.Carts.Add(cart);

                    await _context.SaveChangesAsync();
                }
            }

            return cart;
        }
    }
}