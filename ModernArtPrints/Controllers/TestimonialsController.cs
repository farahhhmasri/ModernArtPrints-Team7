using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;

namespace ModernArtPrints.Controllers
{
    [Authorize]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestimonialsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var hasOrders = await _context.Orders
                .AnyAsync(o => o.UserId == user.Id);

            if (!hasOrders)
            {
                return RedirectToAction("Index", "Profile");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Testimonial testimonial)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var hasOrders = await _context.Orders
                .AnyAsync(o => o.UserId == user.Id);

            if (!hasOrders)
            {
                return RedirectToAction("Index", "Profile");
            }

            if (!ModelState.IsValid)
            {
                return View(testimonial);
            }

            testimonial.UserId = user.Id;
            testimonial.IsApproved = false;
            testimonial.CreatedAt = DateTime.Now;

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your Testimonial has been submitted and is waiting for admin approval.";

            return RedirectToAction("Index", "Profile");
        }
    }
}