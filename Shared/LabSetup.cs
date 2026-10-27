using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Data;
namespace WebApplication1;

public static class LabSetup
{
    public static void AddLab(this WebApplicationBuilder builder, string lab)
    {
        var directory = new DirectoryInfo(builder.Environment.ContentRootPath);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Shared", "database.json"))) directory = directory.Parent;
        var path = directory is null ? Path.Combine(builder.Environment.ContentRootPath, "database.json") : Path.Combine(directory.FullName, "Shared", "database.json");
        builder.Configuration.AddJsonFile(path).AddEnvironmentVariables();
        builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ShopDatabase")), ServiceLifetime.Scoped);
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6; options.Password.RequireDigit = false; options.Password.RequireLowercase = false; options.Password.RequireUppercase = false; options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true; options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders().AddClaimsPrincipalFactory<IdentityClaims>();
        builder.Services.ConfigureApplicationCookie(options => { options.Cookie.Name = "MocBooks." + lab; options.LoginPath = "/Login"; options.AccessDeniedPath = "/AccessDenied"; options.ExpireTimeSpan = TimeSpan.FromHours(8); });
        builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
        builder.Services.AddScoped<IAuthorizationHandler, DatabasePermissionHandler>();
        builder.Services.AddAuthorization(options => { foreach (var permission in Permissions.All) options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission))); });
    }
    public static async Task InitializeLabAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); await db.Database.MigrateAsync();
        await using var seedTransaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("EXEC sp_getapplock @Resource = N'MocBooks.Seed', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 60000");
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { "Admin", "Customer" }) if (!await roles.RoleExistsAsync(role)) Ensure(await roles.CreateAsync(new(role)));
        var admin = await roles.FindByNameAsync("Admin") ?? throw new InvalidOperationException("Missing Admin role");
        if (!(await roles.GetClaimsAsync(admin)).Any()) foreach (var p in Permissions.All) Ensure(await roles.AddClaimAsync(admin, new("Permission", p)));
        if (!await db.Products.AnyAsync())
        {
            foreach (var p in SeedBooks.Items) db.Products.Add(p with { Id = 0 });
            await db.SaveChangesAsync();
        }
        foreach (var name in await db.Products.Select(x => x.Category).Distinct().ToArrayAsync()) if (!await db.Categories.AnyAsync(x => x.Name == name)) db.Categories.Add(new() { Name = name });
        await db.SaveChangesAsync();
        foreach (var p in await db.Products.Where(x => x.CategoryId == null).ToArrayAsync()) { var cat = await db.Categories.FirstAsync(x => x.Name == p.Category); db.Entry(p).Property(x => x.CategoryId).CurrentValue = cat.Id; }
        await db.SaveChangesAsync();
        // Preserve existing password hashes and customer links when upgrading the old labs.
        foreach (var account in await db.Accounts.AsNoTracking().ToArrayAsync())
        {
            if (await users.FindByNameAsync(account.UserName) is not null) continue;
            var user = new ApplicationUser { UserName = account.UserName, Email = account.Email, FullName = account.Name, CustomerId = account.CustomerId, PasswordHash = account.PasswordHash, EmailConfirmed = true };
            Ensure(await users.CreateAsync(user)); Ensure(await users.AddToRoleAsync(user, account.Role == "Admin" ? "Admin" : "Customer"));
        }
        if (app.Environment.EnvironmentName == "Development")
        {
            foreach (var name in new[] { "admin", "khachhang" })
            {
                if (await users.FindByNameAsync(name) is not null) continue;
                Customer? customer = null;
                if (name == "khachhang") { customer = await db.Customers.FirstOrDefaultAsync(x => x.Email == "customer@example.com"); if (customer is null) { customer = new(0, "Khách hàng mẫu", "customer@example.com", "0901234567", "Địa chỉ mẫu"); db.Customers.Add(customer); await db.SaveChangesAsync(); } }
                var user = new ApplicationUser { UserName = name, Email = name == "admin" ? "admin@moc.local" : "customer@example.com", FullName = name == "admin" ? "Quản trị viên" : "Khách hàng mẫu", CustomerId = customer?.Id, EmailConfirmed = true };
                Ensure(await users.CreateAsync(user, "123456")); Ensure(await users.AddToRoleAsync(user, name == "admin" ? "Admin" : "Customer"));
            }
        }
        await seedTransaction.CommitAsync();
    }
    private static void Ensure(IdentityResult result) { if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description))); }
}



