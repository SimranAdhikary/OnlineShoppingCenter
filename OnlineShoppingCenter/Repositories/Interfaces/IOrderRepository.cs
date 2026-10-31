using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.ViewModels.Common;

namespace OnlineShoppingCenter.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CheckoutAsync(int userId, Address shippingAddress);
        Task<PaginatedList<Order>> GetUserOrdersAsync(int userId, int page, int pageSize);
        Task<Order?> GetOrderDetailsAsync(int orderId, int userId);
        Task<PaginatedList<Order>> GetAllOrdersAsync(string? status, int page, int pageSize);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, string status);
    }
}
