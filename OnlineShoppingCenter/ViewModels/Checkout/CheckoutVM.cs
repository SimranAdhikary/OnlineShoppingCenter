using System.ComponentModel.DataAnnotations;
using OnlineShoppingCenter.ViewModels.Cart;

namespace OnlineShoppingCenter.ViewModels.Checkout
{
    public class CheckoutVM
    {
        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public CartVM? Cart { get; set; }
    }
}
