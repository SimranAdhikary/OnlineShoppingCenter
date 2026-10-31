using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Interfaces;
using OnlineShoppingCenter.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShoppingCenter.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepo;
        private readonly ICartRepository _cartRepo;

        public AuthController(IAuthRepository authRepo, ICartRepository cartRepo)
        {
            _authRepo = authRepo;
            _cartRepo = cartRepo;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectBasedOnRole();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authRepo.LoginAsync(model.Email, model.Password, model.RememberMe);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var user = await _authRepo.GetUserByEmailAsync(model.Email);
            var roles = await _authRepo.GetUserRolesAsync(user!);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (roles.Contains("Admin"))
                return Redirect("/Admin/Dashboard/Index");

            return RedirectToAction("Index", "Catalog");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectBasedOnRole();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _authRepo.RegisterAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _authRepo.AddToRoleAsync(user, "Customer");
            await _cartRepo.GetCartAsync(user.Id);

            await _authRepo.LoginAsync(model.Email, model.Password, false);
            return RedirectToAction("Index", "Catalog");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authRepo.LogoutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectBasedOnRole()
        {
            if (User.IsInRole("Admin"))
                return Redirect("/Admin/Dashboard/Index");
            return RedirectToAction("Index", "Catalog");
        }
    }
}
