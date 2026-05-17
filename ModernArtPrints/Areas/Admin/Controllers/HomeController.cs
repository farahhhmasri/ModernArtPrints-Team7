using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var admin = await _userManager.GetUserAsync(User);
            ViewBag.Admin = admin;

            // Basic stats
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();

            // ===== CHART 1: Category Sales Data =====
            // Get sold quantity per category (through OrderItems -> Products -> Categories)
            var categorySales = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Product != null && oi.Product.Category != null)
                .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
                .Select(g => new
                {
                    CategoryName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            ViewBag.CategoryLabels = categorySales.Select(x => x.CategoryName).ToList();
            ViewBag.CategorySalesData = categorySales.Select(x => x.TotalSold).ToList();

            // ===== CHART 2: User Growth Over Time =====
            // Group users by month of registration
            var userGrowth = await _context.Users
                .Where(u => u.CreatedAt != default)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Format as "Jan 2026", "Feb 2026" etc.
            var userGrowthLabels = userGrowth.Select(x =>
                new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy")).ToList();

            // Calculate cumulative count
            var cumulativeCounts = new List<int>();
            int runningTotal = 0;
            foreach (var item in userGrowth)
            {
                runningTotal += item.Count;
                cumulativeCounts.Add(runningTotal);
            }

            ViewBag.UserGrowthLabels = userGrowthLabels;
            ViewBag.UserGrowthData = cumulativeCounts;

            return View();
        }
    }
}