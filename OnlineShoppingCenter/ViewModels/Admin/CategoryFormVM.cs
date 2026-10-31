using System.ComponentModel.DataAnnotations;

namespace OnlineShoppingCenter.ViewModels.Admin
{
    public class CategoryFormVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
