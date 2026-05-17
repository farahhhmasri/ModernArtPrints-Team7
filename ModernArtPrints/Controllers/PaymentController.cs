using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using ModernArtPrints.Models.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace ModernArtPrints.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p.Discount)
                .Where(c => c.Cart.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            decimal subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity);

            decimal discountAmount = 0;
            bool hasFreeShipping = false;
            string discountLabel = "";

            foreach (var item in cartItems)
            {
                var discount = item.Product?.Discount;

                bool isActiveDiscount = discount != null
                    && discount.IsActive
                    && discount.StartDate <= DateTime.Now
                    && discount.EndDate >= DateTime.Now;

                if (!isActiveDiscount) continue;

                switch (discount.DiscountType)
                {
                    case DiscountType.Percentage:
                        discountAmount += (discount.Value / 100) * (item.Product.Price * item.Quantity);
                        discountLabel = $"{discount.Value}% off";
                        break;
                    case DiscountType.FixedAmount:
                        discountAmount += discount.Value * item.Quantity;
                        discountLabel = $"${discount.Value} off per item";
                        break;
                    case DiscountType.FreeShipping:
                        hasFreeShipping = true;
                        discountLabel = "Free Shipping";
                        break;
                    case DiscountType.NoDiscount:
                    default:
                        break;
                }
            }

            discountAmount = Math.Min(discountAmount, subtotal);
            decimal shipping = (subtotal >= 100 || hasFreeShipping) ? 0 : 10;
            decimal total = Math.Max(subtotal - discountAmount + shipping, 0);

            var model = new CheckoutViewModel
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                CartItems = cartItems,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                DiscountLabel = discountLabel,
                HasFreeShipping = hasFreeShipping,
                ShippingCost = shipping,
                Total = total,
                // Pass publishable key to view
                //StripePublishableKey = _stripeSettings.PublishableKey
                StripePublishableKey = _configuration["Stripe:PublishableKey"] ?? ""
            };

            return View(model);
        }


        // Creates the Stripe Checkout Session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p.Discount)
                .Where(c => c.Cart.UserId == user.Id)
                .ToListAsync();

            decimal discountAmount = 0;
            bool hasFreeShipping = false;

            foreach (var item in cartItems)
            {
                var discount = item.Product?.Discount;

                bool isActive = discount != null
                    && discount.IsActive
                    && discount.StartDate <= DateTime.Now
                    && discount.EndDate >= DateTime.Now;

                if (!isActive) continue;

                switch (discount.DiscountType)
                {
                    case DiscountType.Percentage:
                        discountAmount += (discount.Value / 100) * (item.Product.Price * item.Quantity);
                        break;
                    case DiscountType.FixedAmount:
                        discountAmount += discount.Value * item.Quantity;
                        break;
                    case DiscountType.FreeShipping:
                        hasFreeShipping = true;
                        break;
                }
            }

            decimal subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity);
            discountAmount = Math.Min(discountAmount, subtotal);
            decimal shipping = (subtotal >= 100 || hasFreeShipping) ? 0 : 10;

            var lineItems = new List<SessionLineItemOptions>();

            // Add each product with its discounted price baked in
            foreach (var item in cartItems)
            {
                var discount = item.Product?.Discount;

                bool isActive = discount != null
                    && discount.IsActive
                    && discount.StartDate <= DateTime.Now
                    && discount.EndDate >= DateTime.Now;

                decimal unitPrice = item.Product.Price;

                // Apply discount directly to the unit price
                if (isActive)
                {
                    switch (discount.DiscountType)
                    {
                        case DiscountType.Percentage:
                            unitPrice = unitPrice - (unitPrice * discount.Value / 100);
                            break;
                        case DiscountType.FixedAmount:
                            unitPrice = Math.Max(unitPrice - discount.Value, 0);
                            break;
                    }
                }

                // ✅ Make sure unit price is at least 1 cent
                long unitAmountInCents = Math.Max((long)(unitPrice * 100), 1);

                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = unitAmountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Quantity
                });
            }

            // Add shipping as a line item only if it's greater than 0
            if (shipping > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(shipping * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Shipping"
                        }
                    },
                    Quantity = 1
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                CustomerEmail = user.Email,
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Payment/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Payment/Cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }


        // Success page — clears cart after payment
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                TempData["Error"] = "Invalid payment session.";
                return RedirectToAction("Index", "Cart");
            }

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            if (session == null || session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Payment was not completed.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Discount)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            decimal subtotal = 0;
            decimal discountAmount = 0;
            bool hasFreeShipping = false;

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product == null)
                    continue;

                decimal itemOriginalTotal = cartItem.Product.Price * cartItem.Quantity;
                subtotal += itemOriginalTotal;

                var discount = cartItem.Product.Discount;

                bool isActiveDiscount = discount != null
                    && discount.IsActive
                    && discount.StartDate <= DateTime.Now
                    && discount.EndDate >= DateTime.Now;

                if (!isActiveDiscount)
                    continue;

                switch (discount.DiscountType)
                {
                    case DiscountType.Percentage:
                        discountAmount += (discount.Value / 100) * itemOriginalTotal;
                        break;

                    case DiscountType.FixedAmount:
                        discountAmount += discount.Value * cartItem.Quantity;
                        break;

                    case DiscountType.FreeShipping:
                        hasFreeShipping = true;
                        break;
                }
            }

            discountAmount = Math.Min(discountAmount, subtotal);

            decimal shipping = (subtotal >= 100 || hasFreeShipping) ? 0 : 10;
            decimal total = Math.Max(subtotal - discountAmount + shipping, 0);

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                DiscountAmount = discountAmount,
                DiscountId = null,
                OrderStatus = OrderStatus.processing,
                PaymentStatus = PaymentStatus.Success,
                ShippingAddress = user.Address,
                PhoneNumber = user.PhoneNumber,
                UpdatedAt = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product == null)
                    continue;

                decimal unitPrice = cartItem.Product.Price;

                var discount = cartItem.Product.Discount;

                bool isActiveDiscount = discount != null
                    && discount.IsActive
                    && discount.StartDate <= DateTime.Now
                    && discount.EndDate >= DateTime.Now;

                if (isActiveDiscount)
                {
                    switch (discount.DiscountType)
                    {
                        case DiscountType.Percentage:
                            unitPrice = unitPrice - (unitPrice * discount.Value / 100);
                            break;

                        case DiscountType.FixedAmount:
                            unitPrice = Math.Max(unitPrice - discount.Value, 0);
                            break;
                    }
                }

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * cartItem.Quantity
                };

                order.OrderItems.Add(orderItem);

                cartItem.Product.StockQuantity -= cartItem.Quantity;

                if (cartItem.Product.StockQuantity < 0)
                {
                    cartItem.Product.StockQuantity = 0;
                }
            }

            _context.Orders.Add(order);

            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment completed successfully. Your order has been created.";

            return View(order);
        }

        public IActionResult Cancel()
        {
            TempData["Error"] = "Payment was cancelled. Your cart is still saved.";
            return View();
        }
    }
}