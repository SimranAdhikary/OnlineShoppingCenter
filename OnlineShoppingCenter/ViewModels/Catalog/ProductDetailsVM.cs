using OnlineShoppingCenter.Models;

namespace OnlineShoppingCenter.ViewModels.Catalog
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = null!;
        public List<FileEntity> Images { get; set; } = new();
    }
}
