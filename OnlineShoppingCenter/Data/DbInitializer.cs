using OnlineShoppingCenter.Models;
using Microsoft.AspNetCore.Identity;

namespace OnlineShoppingCenter.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new Role { Name = role });
            }
        }
    }
}
