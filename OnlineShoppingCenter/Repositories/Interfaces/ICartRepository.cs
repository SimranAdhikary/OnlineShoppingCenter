using OnlineShoppingCenter.Models;

namespace OnlineShoppingCenter.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetCartAsync(int userId);
        Task AddToCartAsync(int userId, int productId, int quantity = 1);
        Task UpdateQuantityAsync(int userId, int productId, int quantity);
        Task RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
    }
}
