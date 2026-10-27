using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class CustomersModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public List<Customer> Items { get; set; } = [];
    public async Task OnGetAsync() { var q = db.Customers.AsNoTracking(); if (!string.IsNullOrWhiteSpace(Search)) q = q.Where(x => x.Name.Contains(Search) || x.Email.Contains(Search) || x.Phone.Contains(Search)); Items = await q.ToListAsync(); }
    public async Task<IActionResult> OnPostDeleteAsync(int id) { if (await db.Orders.AnyAsync(x => x.CustomerId == id) || await db.Users.AnyAsync(x => x.CustomerId == id) || await db.Accounts.AnyAsync(x => x.CustomerId == id)) { TempData["Message"] = "Không thể xóa khách hàng có tài khoản hoặc đơn hàng."; return RedirectToPage(new { Search }); } var x = await db.Customers.FindAsync(id); if (x is null) return NotFound(); db.Customers.Remove(x); await db.SaveChangesAsync(); TempData["Message"] = "Đã xóa khách hàng."; return RedirectToPage(new { Search }); }
}

