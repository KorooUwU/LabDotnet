using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class ProductsModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public List<Product> Items { get; set; } = [];
    public async Task OnGetAsync() { var q = db.Products.AsNoTracking(); if (!string.IsNullOrWhiteSpace(Search)) q = q.Where(x => x.Name.Contains(Search) || x.Author.Contains(Search) || x.Category.Contains(Search)); Items = await q.ToListAsync(); }
    public async Task<IActionResult> OnPostDeleteAsync(int id) { var x = await db.Products.FindAsync(id); if (x is null) return NotFound(); db.Products.Remove(x); await db.SaveChangesAsync(); TempData["Message"] = "Đã xóa sách."; return RedirectToPage(new { Search }); }
}

