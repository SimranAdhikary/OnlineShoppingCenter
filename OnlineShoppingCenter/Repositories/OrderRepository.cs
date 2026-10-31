using OnlineShoppingCenter.Data;
using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Common;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public OrderRepository(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> CheckoutAsync(int userId, Address shippingAddress)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                    throw new InvalidOperationException("Cart is empty.");

                foreach (var item in cart.Items)
                {
                    if (item.Product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for '{item.Product.Name}'. Available: {item.Product.Stock}.");
                }

                _context.Addresses.Add(shippingAddress);
                await _unitOfWork.SaveChangesAsync();

                var order = new Order
                {
                    UserId = userId,
                    AddressId = shippingAddress.Id,
                    Status = "Pending",
                    TotalAmount = cart.Items.Sum(ci => ci.Product.Price * ci.Quantity)
                };
                _context.Orders.Add(order);
                await _unitOfWork.SaveChangesAsync();

                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                    cartItem.Product.Stock -= cartItem.Quantity;
                }

                _context.CartItems.RemoveRange(cart.Items);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return order;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaginatedList<Order>> GetUserOrdersAsync(int userId, int page, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Items);

            return await PaginatedList<Order>.CreateAsync(query, page, pageSize);
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<PaginatedList<Order>> GetAllOrdersAsync(string? status, int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            return await PaginatedList<Order>.CreateAsync(query, page, pageSize);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.Orders.Update(order);
            }
        }
    }
}
