using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using ModernArtPrints.Models.ViewModels;

namespace ModernArtPrints.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderStatus != OrderStatus.delivered || order.PaymentStatus != PaymentStatus.Success)
            {
                TempData["ErrorMessage"] = "You can add reviews only for delivered and paid orders.";
                return RedirectToAction("Index", "Orders");
            }

            var model = new OrderReviewViewModel
            {
                OrderId = order.Id,
                Products = order.OrderItems.Select(oi => new ProductReviewItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Product",
                    ProductImageUrl = oi.Product?.MainImageUrl
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderReviewViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderStatus != OrderStatus.delivered || order.PaymentStatus != PaymentStatus.Success)
            {
                TempData["ErrorMessage"] = "You can add reviews only for delivered and paid orders.";
                return RedirectToAction("Index", "Orders");
            }

            var orderProductIds = order.OrderItems.Select(oi => oi.ProductId).ToList();

            var submittedReviews = model.Products
                .Where(p =>
                    orderProductIds.Contains(p.ProductId) &&
                    p.Rating.HasValue &&
                    p.Rating.Value >= 1 &&
                    p.Rating.Value <= 5 &&
                    !string.IsNullOrWhiteSpace(p.CommentText))
                .ToList();

            if (!submittedReviews.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one review with rating and comment.");

                var postedProducts = model.Products ?? new List<ProductReviewItemViewModel>();

                var orderItems = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.OrderId == model.OrderId)
                    .ToListAsync();

                model.Products = orderItems.Select(oi =>
                {
                    var postedItem = postedProducts.FirstOrDefault(p => p.ProductId == oi.ProductId);

                    return new ProductReviewItemViewModel
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? "Product",
                        ProductImageUrl = oi.Product?.MainImageUrl,
                        Rating = postedItem?.Rating,
                        CommentText = postedItem?.CommentText
                    };
                }).ToList();

                return View(model);
            }

            var alreadyReviewedProducts = new List<string>();
            var addedReviewsCount = 0;

            foreach (var item in submittedReviews)
            {
                var alreadyReviewed = await _context.ProductReviews
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r =>
                        r.ProductId == item.ProductId &&
                        r.UserId == user.Id);

                if (alreadyReviewed != null)
                {
                    var productName = alreadyReviewed.Product?.Name ?? "a product";
                    alreadyReviewedProducts.Add(productName);
                    continue;
                }

                var review = new ProductReview
                {
                    ProductId = item.ProductId,
                    UserId = user.Id,
                    Rating = item.Rating!.Value,
                    CommentText = item.CommentText!.Trim(),
                    IsApproved = false,
                    CreatedAt = DateTime.Now
                };

                _context.ProductReviews.Add(review);
                addedReviewsCount++;
            }

            if (addedReviewsCount == 0 && alreadyReviewedProducts.Any())
            {
                TempData["ErrorMessage"] = "You already reviewed: " + string.Join(", ", alreadyReviewedProducts);
                return RedirectToAction("Index", "Orders");
            }

            await _context.SaveChangesAsync();

            if (alreadyReviewedProducts.Any())
            {
                TempData["SuccessMessage"] =
                    "Your new reviews were submitted. You already reviewed: " + string.Join(", ", alreadyReviewedProducts);
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Your reviews have been submitted and are waiting for admin approval.";
            }

            return RedirectToAction("Index", "Orders");
        }
    }
}