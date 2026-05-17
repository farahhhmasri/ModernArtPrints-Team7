using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimonialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Testimonials
        public async Task<IActionResult> Index(string? searchString, bool? isApproved)
        {
            var testimonials = _context.Testimonials
                .Include(t => t.User)
                .AsQueryable();

            // Search by content or user
            if (!string.IsNullOrEmpty(searchString))
            {
                testimonials = testimonials.Where(t =>
                    t.Message.Contains(searchString) ||
                    t.User.UserName.Contains(searchString));
            }

            // Filter by approval status
            if (isApproved.HasValue)
            {
                testimonials = testimonials.Where(t => t.IsApproved == isApproved.Value);
            }

            ViewData["SearchString"] = searchString;
            ViewData["IsApproved"] = isApproved;

            return View(await testimonials.OrderByDescending(t => t.CreatedAt).ToListAsync());
        }

        // POST: Admin/Testimonials/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = true;
            _context.Update(testimonial);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Testimonial approved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Testimonials/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = false;
            _context.Update(testimonial);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Testimonial rejected successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Testimonials/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                _context.Testimonials.Remove(testimonial);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Testimonial deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}