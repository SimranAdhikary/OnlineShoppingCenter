using OnlineShoppingCenter.Data;
using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Files)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity = 1)
        {
            var cart = await GetCartAsync(userId);
            var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(item);
            }
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await GetCartAsync(userId);
            var item = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

            if (item == null) return;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                _context.CartItems.Update(item);
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await GetCartAsync(userId);
            var item = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

            if (item != null)
                _context.CartItems.Remove(item);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
                _context.CartItems.RemoveRange(cart.Items);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.Sum(ci => ci.Quantity) ?? 0;
        }
    }
}
