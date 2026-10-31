using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.ViewModels.Common;

namespace OnlineShoppingCenter.ViewModels.Catalog
{
    public class ProductListVM
    {
        public PaginatedList<Product> Products { get; set; } = null!;
        public List<Category> Categories { get; set; } = new();
        public int? CategoryId { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; }
    }
}
