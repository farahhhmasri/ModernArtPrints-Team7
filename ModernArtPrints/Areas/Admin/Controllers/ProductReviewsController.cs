using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]
   
    public class ProductReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductReviews
        public async Task<IActionResult> Index(string? searchString, bool? isApproved, int? productId)
        {
            var reviews = _context.ProductReviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();

            // Search by comment or user
            if (!string.IsNullOrEmpty(searchString))
            {
                reviews = reviews.Where(r =>
                    r.CommentText.Contains(searchString) ||
                    r.User.UserName.Contains(searchString) ||
                    r.Product.Name.Contains(searchString));
            }

            // Filter by approval status
            if (isApproved.HasValue)
            {
                reviews = reviews.Where(r => r.IsApproved == isApproved.Value);
            }

            // Filter by product
            if (productId.HasValue)
            {
                reviews = reviews.Where(r => r.ProductId == productId.Value);
            }

            ViewData["SearchString"] = searchString;
            ViewData["IsApproved"] = isApproved;
            ViewData["ProductId"] = productId;

            // Load products for filter dropdown
            ViewBag.Products = await _context.Products
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return View(await reviews.OrderByDescending(r => r.CreatedAt).ToListAsync());
        }

        // POST: Admin/ProductReviews/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.IsApproved = true;
            _context.Update(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review approved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ProductReviews/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.IsApproved = false;
            _context.Update(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review rejected successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ProductReviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Review deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}