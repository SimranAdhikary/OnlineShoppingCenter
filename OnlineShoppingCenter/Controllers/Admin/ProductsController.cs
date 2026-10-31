using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Helpers;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ProductsController : Controller
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(
            IRepository<Product> productRepo,
            IRepository<Category> categoryRepo,
            IFileRepository fileRepo,
            IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _fileRepo = fileRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Files)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Products/Index.cshtml", products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormVM
            {
                Categories = await _categoryRepo.GetAll().ToListAsync()
            };
            return View("~/Views/Admin/Products/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepo.GetAll().ToListAsync();
                return View("~/Views/Admin/Products/Create.cshtml", model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                CategoryId = model.CategoryId
            };

            await _productRepo.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            if (model.Files != null && model.Files.Count > 0)
            {
                var validation = FileValidator.Validate(model.Files);
                if (!validation.IsValid)
                {
                    TempData["Error"] = validation.Error;
                    return RedirectToAction("Edit", new { id = product.Id });
                }

                await _fileRepo.UploadFilesAsync(model.Files, "Product", product.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            TempData["Success"] = "Product created.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();

            var model = new ProductFormVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                Categories = await _categoryRepo.GetAll().ToListAsync(),
                ExistingFiles = await _fileRepo.GetFilesByOwnerAsync("Product", product.Id)
            };

            return View("~/Views/Admin/Products/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepo.GetAll().ToListAsync();
                model.ExistingFiles = await _fileRepo.GetFilesByOwnerAsync("Product", model.Id);
                return View("~/Views/Admin/Products/Edit.cshtml", model);
            }

            var product = await _productRepo.GetByIdAsync(model.Id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.CategoryId = model.CategoryId;
            _productRepo.Update(product);

            if (model.Files != null && model.Files.Count > 0)
            {
                var validation = FileValidator.Validate(model.Files);
                if (!validation.IsValid)
                {
                    TempData["Error"] = validation.Error;
                    model.Categories = await _categoryRepo.GetAll().ToListAsync();
                    model.ExistingFiles = await _fileRepo.GetFilesByOwnerAsync("Product", model.Id);
                    return View("~/Views/Admin/Products/Edit.cshtml", model);
                }

                await _fileRepo.ReplaceFilesAsync(model.Files, "Product", product.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Product updated.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.OrderItems)
                .Include(p => p.CartItems)
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return View("~/Views/Admin/Products/Delete.cshtml", product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepo.GetAll()
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (product.OrderItems.Any())
            {
                TempData["Error"] = $"Cannot delete \"{product.Name}\" because it is referenced by {product.OrderItems.Count} order(s). Products with order history cannot be removed.";
                return RedirectToAction("Index");
            }

            await _fileRepo.DeleteFilesByOwnerAsync("Product", id);
            _productRepo.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Index");
        }
    }
}
