using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.ViewModels.Common;

namespace OnlineShoppingCenter.ViewModels.Orders
{
    public class OrderListVM
    {
        public PaginatedList<Order> Orders { get; set; } = null!;
    }
}
