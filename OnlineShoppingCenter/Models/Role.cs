using Microsoft.AspNetCore.Identity;

namespace OnlineShoppingCenter.Models
{
    public class Role : IdentityRole<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
