using OnlineShoppingCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<FileEntity> Files => Set<FileEntity>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User
            builder.Entity<User>(e =>
            {
                e.HasOne(u => u.Cart)
                    .WithOne(c => c.User)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(u => u.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(u => u.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Category
            builder.Entity<Category>(e =>
            {
                e.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product
            builder.Entity<Product>(e =>
            {
                e.Property(p => p.Price).HasColumnType("decimal(18,2)");
                e.HasIndex(p => p.CategoryId);
                e.HasIndex(p => p.Name);

                e.HasMany(p => p.Files)
                    .WithOne()
                    .HasForeignKey(f => f.OwnerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Cart
            builder.Entity<Cart>(e =>
            {
                e.HasIndex(c => c.UserId).IsUnique();
            });

            // CartItem
            builder.Entity<CartItem>(e =>
            {
                e.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

                e.HasOne(ci => ci.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(ci => ci.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Order
            builder.Entity<Order>(e =>
            {
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasIndex(o => o.UserId);
                e.HasIndex(o => o.Status);

                e.HasOne(o => o.Address)
                    .WithMany(a => a.Orders)
                    .HasForeignKey(o => o.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem
            builder.Entity<OrderItem>(e =>
            {
                e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
                e.HasIndex(oi => oi.OrderId);

                e.HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Address
            builder.Entity<Address>(e =>
            {
                e.HasIndex(a => a.UserId);
            });

            // FileEntity
            builder.Entity<FileEntity>(e =>
            {
                e.HasIndex(f => new { f.OwnerType, f.OwnerId });
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                {
                    if (entry.Entity is BaseEntity baseEntity)
                    {
                        baseEntity.UpdatedAt = now;
                        if (entry.State == EntityState.Added)
                            baseEntity.CreatedAt = now;
                    }

                    if (entry.Entity is User user)
                    {
                        user.UpdatedAt = now;
                        if (entry.State == EntityState.Added)
                            user.CreatedAt = now;
                    }

                    if (entry.Entity is Role role)
                    {
                        role.UpdatedAt = now;
                        if (entry.State == EntityState.Added)
                            role.CreatedAt = now;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
