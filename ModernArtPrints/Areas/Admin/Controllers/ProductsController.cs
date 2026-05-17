using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Areas.Admin.Models;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]


    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string searchString)
        {
            IQueryable<Product> products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Discount);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                products = products.Where(p =>
                    p.Name.ToLower().Contains(searchString.ToLower()));
            }

            ViewData["SearchString"] = searchString;

            return View(await products.ToListAsync());
        }
        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Discount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["DiscountId"] = new SelectList(_context.Discounts, "Id", "Code");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // 1. إنشاء كائن المنتج
                var product = new Product
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    Price = vm.Price,
                    StockQuantity = vm.StockQuantity,
                    CategoryId = vm.CategoryId,
                    CreatedAt = DateTime.Now,
                    DiscountId = vm.DiscountId,
                    IsActive = true
                };

                // 2. معالجة الصورة الرئيسية (MainImageUrl)
                if (vm.MainImageFile != null)
                {
                    product.MainImageUrl = await SaveImage(vm.MainImageFile);
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // نحتاج الـ ID هنا لربط الصور الإضافية

                // 3. معالجة الصور الإضافية (ProductImages)
                if (vm.AdditionalImages != null && vm.AdditionalImages.Any())
                {
                    foreach (var image in vm.AdditionalImages)
                    {
                        var imageUrl = await SaveImage(image);
                        var productImage = new ProductImage
                        {
                            ImageUrl = imageUrl,
                            ProductId = product.Id
                        };
                        _context.ProductImages.Add(productImage);
                    }
                    await _context.SaveChangesAsync();
                }


                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }


        private async Task<string> SaveImage(IFormFile file)
        {
            // Generate unique image name
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Physical path
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products");

            // Create uploads folder if it does not exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Full physical file path
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Save image into uploads folder
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path to save in database
            return "/uploads/products/" + fileName;
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Populate ViewBags for dropdowns
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.DiscountId = new SelectList(_context.Discounts, "Id", "Code", product.DiscountId);

            // Return the product as the model — THIS WAS MISSING!
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? MainImageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null) return NotFound();

                    // Update fields
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.DiscountId = product.DiscountId;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.CategoryId = product.CategoryId;

                    // Handle image upload
                    if (MainImageFile != null)
                    {
                        existingProduct.MainImageUrl = await SaveImage(MainImageFile);
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Product Updated Successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.DiscountId = new SelectList(_context.Discounts, "Id", "Code", product.DiscountId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Discount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}