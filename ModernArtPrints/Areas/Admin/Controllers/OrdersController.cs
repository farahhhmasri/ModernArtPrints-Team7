using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]
  

    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string searchString, string orderStatus, string paymentStatus)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Discount)
                .AsQueryable();

            // Search by user email, name, or order ID
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    (o.User != null && o.User.Email.Contains(searchString)) ||
                    (o.User != null && (o.User.FullName).Contains(searchString)) ||
                    o.Id.ToString().Contains(searchString)
                );
            }

            // Filter by Order Status (enum)
            if (!string.IsNullOrEmpty(orderStatus) && Enum.TryParse<OrderStatus>(orderStatus, true, out var statusEnum))
            {
                orders = orders.Where(o => o.OrderStatus == statusEnum);
            }

            // Filter by Payment Status (enum)
            if (!string.IsNullOrEmpty(paymentStatus) && Enum.TryParse < PaymentStatus > (paymentStatus, true, out var paymentEnum))
    {
                orders = orders.Where(o => o.PaymentStatus == paymentEnum);
            }

            // Preserve filter values in ViewData for the view
            ViewData["SearchString"] = searchString;
            ViewData["OrderStatus"] = orderStatus;
            ViewData["PaymentStatus"] = paymentStatus;

            return View(await orders.OrderByDescending(o => o.OrderDate).ToListAsync());
        }
        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Discount)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewData["DiscountId"] = new SelectList(_context.Discounts, "Id", "Code");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderDate,TotalAmount,DiscountAmount,DiscountId,OrderStatus,PaymentStatus,ShippingAddress,PhoneNumber,UpdatedAt")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DiscountId"] = new SelectList(_context.Discounts, "Id", "Code", order.DiscountId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["DiscountId"] = new SelectList(_context.Discounts, "Id", "Code", order.DiscountId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            var existingOrder = await _context.Orders.FindAsync(id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingOrder.OrderStatus = order.OrderStatus;
                existingOrder.PaymentStatus = order.PaymentStatus;
                existingOrder.UpdatedAt = DateTime.Now;

                // اختياري إذا بدك الأدمن يعدلهم
                existingOrder.ShippingAddress = order.ShippingAddress;
                existingOrder.PhoneNumber = order.PhoneNumber;

                await _context.SaveChangesAsync();

                TempData["success"] = "Order updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Discount)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
