using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace WebApplication1.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    // Chỉ giữ để chuyển tài khoản cũ sang Identity, không dùng để đăng nhập.
    public DbSet<UserAccount> Accounts => Set<UserAccount>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
        model.Entity<Product>().Property(x => x.ImageUrl).HasMaxLength(300);
        model.Entity<Product>().Property(x => x.Author).HasMaxLength(120);
        model.Entity<Product>().Property(x => x.Publisher).HasMaxLength(120);
        model.Entity<Product>().Property(x => x.Isbn).HasMaxLength(13);
        model.Entity<Product>().HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Category>().HasIndex(x => x.Name).IsUnique();
        model.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        model.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        model.Entity<ApplicationUser>().HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ApplicationUser>().HasIndex(x => x.CustomerId).IsUnique().HasFilter("[CustomerId] IS NOT NULL");
        model.Entity<Order>().Property(x => x.PaymentMethod).HasMaxLength(30);
        model.Entity<Order>().Property(x => x.CheckoutToken).HasMaxLength(64);
        model.Entity<Order>().HasIndex(x => x.CheckoutToken).IsUnique().HasFilter("[CheckoutToken] IS NOT NULL");
        model.Entity<UserAccount>().HasIndex(x => x.Email).IsUnique();
        model.Entity<UserAccount>().HasIndex(x => x.UserName).IsUnique();
        model.Entity<UserAccount>().Property(x => x.UserName).HasMaxLength(50).IsRequired();
        model.Entity<UserAccount>().Property(x => x.Email).HasMaxLength(200);
        model.Entity<UserAccount>().Property(x => x.Name).HasMaxLength(120);
        model.Entity<UserAccount>().Property(x => x.Role).HasMaxLength(20);
        model.Entity<UserAccount>().HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
