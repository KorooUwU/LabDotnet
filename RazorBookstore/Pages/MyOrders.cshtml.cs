using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages;

public class MyOrdersModel(AppDbContext db, UserManager<ApplicationUser> users) : PageModel
{
    public List<Order> Orders { get; private set; } = [];
    public async Task OnGetAsync(int? id) { var user = await users.GetUserAsync(User); if (user?.CustomerId is not int customerId) return; var query = db.Orders.AsNoTracking().Include(x => x.Lines).Where(x => x.CustomerId == customerId); if (id.HasValue) query = query.Where(x => x.Id == id); Orders = await query.OrderByDescending(x => x.Id).ToListAsync(); }
}

