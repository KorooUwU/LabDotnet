using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class OrdersModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    public List<Order> Items { get; set; } = [];
    public async Task OnGetAsync() { var q = db.Orders.AsNoTracking().Include(x => x.Customer).Include(x => x.Lines).AsQueryable(); if (!string.IsNullOrWhiteSpace(Search)) q = q.Where(x => x.Customer.Name.Contains(Search) || x.Customer.Email.Contains(Search) || x.Id.ToString() == Search); if (!string.IsNullOrWhiteSpace(Status)) q = q.Where(x => x.Status == Status); Items = await q.OrderByDescending(x => x.Id).ToListAsync(); }
    public async Task<IActionResult> OnPostDeleteAsync(int id) { var x = await db.Orders.FindAsync(id); if (x is null) return NotFound(); db.Orders.Remove(x); await db.SaveChangesAsync(); return RedirectToPage(); }
    public async Task<IActionResult> OnPostStatusAsync(int id, string status) { if (!OrderStatuses.All.ContainsKey(status)) return BadRequest(); var x = await db.Orders.FindAsync(id); if (x is null) return NotFound(); x.Status = status; await db.SaveChangesAsync(); return RedirectToPage(); }
}

