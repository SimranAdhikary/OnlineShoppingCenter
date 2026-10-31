using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class CategoriesController : Controller
    {
        private readonly IRepository<Category> _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IRepository<Category> categoryRepo, IUnitOfWork unitOfWork)
        {
            _categoryRepo = categoryRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAll().ToListAsync();
            return View("~/Views/Admin/Categories/Index.cshtml", categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Categories/Create.cshtml", new CategoryFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormVM model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Categories/Create.cshtml", model);

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description
            };

            await _categoryRepo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Category created.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var model = new CategoryFormVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View("~/Views/Admin/Categories/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormVM model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Categories/Edit.cshtml", model);

            var category = await _categoryRepo.GetByIdAsync(model.Id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;
            _categoryRepo.Update(category);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Category updated.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepo.GetAll()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View("~/Views/Admin/Categories/Delete.cshtml", category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepo.GetAll()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] = $"Cannot delete \"{category.Name}\" because it has {category.Products.Count} product(s). Remove or reassign them first.";
                return RedirectToAction("Index");
            }

            _categoryRepo.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }
    }
}
