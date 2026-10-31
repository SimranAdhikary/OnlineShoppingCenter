namespace OnlineShoppingCenter.ViewModels.Cart
{
    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
