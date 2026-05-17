using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using ModernArtPrints.Models.ViewModels;

namespace ModernArtPrints.Controllers
{   
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["HeaderType"] = "includeNavbar";

            var testimonials = await _context.Testimonials
                .Where(t => t.IsApproved)
                .OrderByDescending(t => t.CreatedAt)
                .Take(4)
                .Include(t => t.User)
                .ToListAsync();

            var saleProducts = await _context.Products
                .Include(p => p.Discount)
                .Where(p =>
                    p.IsActive &&
                    p.DiscountId != 6 && 
                    p.Discount != null &&
                    p.Discount.IsActive &&
                    p.Discount.StartDate <= DateTime.Now &&
                    p.Discount.EndDate >= DateTime.Now)
                .ToListAsync();

            var model = new HomeViewModel
            {
                Testimonials = testimonials,
                SaleProducts = saleProducts,
                Categories = await _context.Categories.ToListAsync()
            };

            return View(model);
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("~/Areas/Identity/Pages/Account/Login.cshtml");
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("~/Areas/Identity/Pages/Account/Register.cshtml");
        }


        public IActionResult Profile()
        {
            return View();
        }



    }
}
