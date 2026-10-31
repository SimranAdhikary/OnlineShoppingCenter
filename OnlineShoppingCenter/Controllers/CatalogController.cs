using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Catalog;
using OnlineShoppingCenter.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Category> _categoryRepo;

        public CatalogController(IRepository<Product> productRepo, IRepository<Category> categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index(int? categoryId, string? q, string? sort, int page = 1)
        {
            if (User.IsInRole("Admin"))
                return Redirect("/Admin/Dashboard/Index");

            var query = _productRepo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Files)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || (p.Description != null && p.Description.Contains(q)));

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = await PaginatedList<Product>.CreateAsync(query, page, 9);
            var categories = await _categoryRepo.GetAll().ToListAsync();

            var vm = new ProductListVM
            {
                Products = products,
                Categories = categories,
                CategoryId = categoryId,
                SearchQuery = q,
                SortBy = sort
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var vm = new ProductDetailsVM
            {
                Product = product,
                Images = product.Files.Where(f => f.Type == "image").ToList()
            };

            return View(vm);
        }
    }
}
