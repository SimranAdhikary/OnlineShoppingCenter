using System.ComponentModel.DataAnnotations;
using OnlineShoppingCenter.Models;

namespace OnlineShoppingCenter.ViewModels.Admin
{
    public class ProductFormVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new();
        public List<FileEntity> ExistingFiles { get; set; } = new();
        public IFormFileCollection? Files { get; set; }
    }
}
