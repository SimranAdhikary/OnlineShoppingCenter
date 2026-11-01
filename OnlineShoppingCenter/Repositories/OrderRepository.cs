using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShoppingCenter.Data;
using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.Services;
using OnlineShoppingCenter.ViewModels.Common;
using System.Buffers.Text;
using System.Net;
using System.Text;

namespace OnlineShoppingCenter.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailService _emailService;
        private readonly UserManager<User> _userManager;

        public OrderRepository(AppDbContext context, IUnitOfWork unitOfWork, EmailService emailService, UserManager<User> userManager)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<Order> CheckoutAsync(int userId, Address shippingAddress, string baseUrl)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var currentUser = await _userManager.FindByIdAsync(userId.ToString());
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
                SendConfirmation(order, cart.Items, currentUser, baseUrl);

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

        private async Task<string> SendConfirmation(Order order, IEnumerable<CartItem> items, User currentUser, string baseUrl)
        {
            string fullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.Email))
            {
                throw new InvalidOperationException(
                    "Current user or email address not found.");
            }

            // Generate dynamic HTML table
            string itemsTable = GenerateOrderItemsTable(items);

            // Build an absolute URL because email clients cannot reliably
            // navigate using a relative URL such as Orders/Details/123.
            string orderId = order.Id.ToString();
            string orderDetailsUrl = $"{baseUrl}/Orders/Details/{orderId}";
            string safeFullName = WebUtility.HtmlEncode(fullName);

            var email = new EmailMessage
            {
                Subject = "Order Confirmation",

                HtmlContent = $@"
                 <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin:0; padding:0; background-color:#f4f6f9;
                             font-family:Arial, Helvetica, sans-serif;'>

                    <div style='max-width:650px; margin:30px auto; background-color:#ffffff;
                                border-radius:10px; overflow:hidden;'>

                        <!-- Branding Header -->
                        <div style='background-color:#1e3a8a; color:#ffffff;
                                    padding:30px 20px; text-align:center;'>

                            <h1 style='margin:0; font-size:26px; font-weight:bold;'>
                                Online Shopping Centre
                            </h1>

                            <p style='margin:8px 0 0; font-size:14px; color:#dbeafe;'>
                                Your Shopping, Our Priority
                            </p>
                        </div>

                        <!-- Email Content -->
                        <div style='padding:30px 25px; color:#333333;'>

                            <h2 style='color:#1e3a8a; margin-top:0;'>
                                Order Confirmation
                            </h2>

                            <p style='font-size:16px; line-height:1.6;'>
                                Hello <strong>{safeFullName}</strong>,
                            </p>

                            <p style='font-size:15px; line-height:1.6;'>
                                Thank you for shopping with
                                <strong>Online Shopping Centre</strong>!
                                Your order has been successfully placed.
                                Below is your order summary.
                            </p>

                            <p style='font-size:15px;'>
                                <strong>Order ID:</strong> #{orderId}
                            </p>

                            <!-- Dynamic Order Items Table -->
                            {itemsTable}

                            <!-- Order Details Button -->
                            <div style='text-align:center; margin:35px 0 20px;'>

                                <a href='{orderDetailsUrl}'
                                   style='display:inline-block;
                                          background-color:#2563eb;
                                          color:#ffffff;
                                          padding:16px 35px;
                                          font-size:16px;
                                          font-weight:bold;
                                          text-decoration:none;
                                          border-radius:8px;'>
                                    View Order Details
                                </a>

                            </div>

                            <p style='font-size:14px; color:#666666; line-height:1.6;'>
                                You can click the button above to view your order details
                                and track your purchase.
                            </p>

                            <p style='font-size:15px; margin-top:25px;'>
                                Thank you for choosing us!
                            </p>

                            <p style='font-size:15px;'>
                                Best regards,<br>
                                <strong>Online Shopping Centre Team</strong>
                            </p>

                        </div>

                        <!-- Footer -->
                        <div style='background-color:#f1f5f9; padding:18px;
                                    text-align:center; color:#64748b; font-size:12px;'>

                            &copy; {DateTime.UtcNow.Year} Online Shopping Centre.
                            All rights reserved.

                        </div>

                    </div>

                </body>
                </html>",

                Recipients = new List<EmailRecipient>
            {
                new EmailRecipient
                {
                    Email = currentUser.Email,
                    Name = fullName
                }
            }
            };


            await _emailService.SendEmailAsync(email);

            return "Email Sent successfully";
        }

        private string GenerateOrderItemsTable(
            IEnumerable<CartItem> items)
        {
            var html = new StringBuilder();

            decimal grandTotal = 0;
            int serialNumber = 1;

            html.Append(@"
            <table width='100%' cellpadding='10' cellspacing='0'
               style='border-collapse:collapse;
                      font-family:Arial,sans-serif;
                      font-size:14px;
                      border:1px solid #ddd;'>

            <thead>
                <tr style='background-color:#f2f2f2;'>
                    <th style='border:1px solid #ddd;'>S.N</th>
                    <th style='border:1px solid #ddd;'>Item Name</th>
                    <th style='border:1px solid #ddd;'>Item Quantity</th>
                    <th style='border:1px solid #ddd;'>Item Price</th>
                    <th style='border:1px solid #ddd;'>Total</th>
                </tr>
            </thead>

            <tbody>
            ");

            foreach (var item in items)
            {
                decimal itemTotal = item.Quantity * item.Product.Price;
                grandTotal += itemTotal;

                html.Append($@"
                <tr>
                    <td style='border:1px solid #ddd;text-align:center;'>
                        {serialNumber}
                    </td>

                    <td style='border:1px solid #ddd;'>
                        {WebUtility.HtmlEncode(item.Product.Name)}
                    </td>

                    <td style='border:1px solid #ddd;text-align:center;'>
                        {item.Quantity}
                    </td>

                    <td style='border:1px solid #ddd;text-align:right;'>
                        {item.Product.Price:N2}
                    </td>

                    <td style='border:1px solid #ddd;text-align:right;'>
                        {itemTotal:N2}
                    </td>
                </tr>
                ");

                serialNumber++;
            }

            html.Append($@"
            <tr style='background-color:#f2f2f2;font-weight:bold;'>
                <td colspan='4'
                    style='border:1px solid #ddd;text-align:right;'>
                    Grand Total:
                </td>

                <td style='border:1px solid #ddd;text-align:right;'>
                    {grandTotal:N2}
                </td>
            </tr>

            </tbody>
            </table>
            ");

            return html.ToString();
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
