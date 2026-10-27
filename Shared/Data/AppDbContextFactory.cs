using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace WebApplication1.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Shared", "database.json"))) directory = directory.Parent;
        var file = directory is null ? Path.Combine(Directory.GetCurrentDirectory(), "database.json") : Path.Combine(directory.FullName, "Shared", "database.json");
        var configuration = new ConfigurationBuilder().AddJsonFile(file).AddEnvironmentVariables().Build();
        return new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(configuration.GetConnectionString("ShopDatabase")).Options);
    }
}
