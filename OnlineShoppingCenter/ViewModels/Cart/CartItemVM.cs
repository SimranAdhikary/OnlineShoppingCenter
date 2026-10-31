namespace OnlineShoppingCenter.ViewModels.Cart
{
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public int Stock { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
