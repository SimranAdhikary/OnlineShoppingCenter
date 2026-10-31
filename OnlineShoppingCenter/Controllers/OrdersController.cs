using System.Security.Claims;
using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Cart;
using OnlineShoppingCenter.ViewModels.Checkout;
using OnlineShoppingCenter.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShoppingCenter.Controllers
{
    [Authorize(Roles = "Customer")]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;

        public OrdersController(IOrderRepository orderRepo, ICartRepository cartRepo)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await _cartRepo.GetCartAsync(GetUserId());

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutVM
            {
                Cart = new CartVM
                {
                    Items = cart.Items.Select(ci => new CartItemVM
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        UnitPrice = ci.Product.Price,
                        Quantity = ci.Quantity,
                        ImagePath = ci.Product.Files.FirstOrDefault(f => f.Type == "image")?.Path
                    }).ToList()
                }
            };

            return View("~/Views/Checkout/Index.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                var cart = await _cartRepo.GetCartAsync(userId);
                model.Cart = new CartVM
                {
                    Items = cart.Items.Select(ci => new CartItemVM
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        UnitPrice = ci.Product.Price,
                        Quantity = ci.Quantity
                    }).ToList()
                };
                return View("~/Views/Checkout/Index.cshtml", model);
            }

            try
            {
                var address = new Address
                {
                    UserId = userId,
                    Street = model.Street,
                    City = model.City,
                    State = model.State,
                    ZipCode = model.ZipCode,
                    Country = model.Country
                };

                var order = await _orderRepo.CheckoutAsync(userId, address);
                TempData["Success"] = "Order placed successfully!";
                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Checkout");
            }
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var orders = await _orderRepo.GetUserOrdersAsync(GetUserId(), page, 10);
            return View(new OrderListVM { Orders = orders });
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepo.GetOrderDetailsAsync(id, GetUserId());
            if (order == null)
                return NotFound();

            return View(new OrderDetailsVM { Order = order });
        }
    }
}
