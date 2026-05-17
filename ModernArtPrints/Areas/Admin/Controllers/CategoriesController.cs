using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Areas.Admin.Models;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModernArtPrints.Areas.Admin.Controllers
{
    [Area("Admin")]
    


    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index(string searchString)

        {
            var categories = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c =>
                    c.Name.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;

            return View(categories.ToList());
          
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    CreatedAt = DateTime.Now //
                };

                if (vm.ImageFile != null)
                {
                    
                    category.ImageUrl = await SaveImage(vm.ImageFile);
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["success"] = "Category Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }
        private async Task<string> SaveImage(IFormFile file)
        {
           
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string fullPath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var viewModel = new CategoryEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ExistingImageUrl = category.ImageUrl 
            };

            return View(viewModel);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound();

                category.Name = vm.Name;
                category.Description = vm.Description;

                
                if (vm.NewImageFile != null)
                {
                   
                    category.ImageUrl = await SaveImage(vm.NewImageFile);
                }
                

                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["success"] = "Category Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }
        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", category.ImageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                    _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Category Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
