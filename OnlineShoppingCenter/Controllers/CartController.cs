using System.Security.Claims;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShoppingCenter.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CartController(ICartRepository cartRepo, IUnitOfWork unitOfWork)
        {
            _cartRepo = cartRepo;
            _unitOfWork = unitOfWork;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Index()
        {
            var cart = await _cartRepo.GetCartAsync(GetUserId());

            var vm = new CartVM
            {
                Items = cart.Items.Select(ci => new CartItemVM
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.Product.Price,
                    Quantity = ci.Quantity,
                    Stock = ci.Product.Stock,
                    ImagePath = ci.Product.Files.FirstOrDefault(f => f.Type == "image")?.Path
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            await _cartRepo.AddToCartAsync(GetUserId(), productId);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Item added to cart.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            await _cartRepo.UpdateQuantityAsync(GetUserId(), productId, quantity);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            await _cartRepo.RemoveFromCartAsync(GetUserId(), productId);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }
    }
}
