using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Admin;
using OnlineShoppingCenter.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShoppingCenter.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Orders/[action]")]
    public class AdminOrdersController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public AdminOrdersController(IOrderRepository orderRepo, IUnitOfWork unitOfWork)
        {
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            var orders = await _orderRepo.GetAllOrdersAsync(status, page, 15);
            var vm = new AdminOrderVM
            {
                Orders = orders,
                StatusFilter = status
            };
            return View("~/Views/Admin/Orders/Index.cshtml", vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return View("~/Views/Admin/Orders/Details.cshtml", new OrderDetailsVM { Order = order });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            await _orderRepo.UpdateOrderStatusAsync(id, status);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Order status updated.";
            return RedirectToAction("Details", new { id });
        }
    }
}
