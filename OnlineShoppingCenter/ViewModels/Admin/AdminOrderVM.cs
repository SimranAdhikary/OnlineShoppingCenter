using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.ViewModels.Common;

namespace OnlineShoppingCenter.ViewModels.Admin
{
    public class AdminOrderVM
    {
        public PaginatedList<Order> Orders { get; set; } = null!;
        public string? StatusFilter { get; set; }

        public static readonly string[] Statuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
    }
}
