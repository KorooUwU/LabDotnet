using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class DashboardModel(AppDbContext db) : PageModel
{
    public int Products { get; set; }
    public int Customers { get; set; }
    public int Categories { get; set; }
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
    public async Task OnGetAsync() { Products = await db.Products.CountAsync(); Customers = await db.Customers.CountAsync(); Categories = await db.Categories.CountAsync(); Orders = await db.Orders.CountAsync(); Revenue = await db.Set<OrderLine>().Where(x => x.Order.Status == "Completed").SumAsync(x => (decimal?)x.UnitPrice * x.Quantity) ?? 0; }
}

