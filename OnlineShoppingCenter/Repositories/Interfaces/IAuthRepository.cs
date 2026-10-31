using OnlineShoppingCenter.Models;
using Microsoft.AspNetCore.Identity;

namespace OnlineShoppingCenter.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterAsync(User user, string password);
        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task LogoutAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task AddToRoleAsync(User user, string role);
    }
}
