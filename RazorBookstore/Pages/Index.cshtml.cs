using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    public List<Product> Books { get; private set; } = [];
    public List<Category> Categories { get; private set; } = [];
    public async Task OnGetAsync() { Categories = await db.Categories.AsNoTracking().ToListAsync(); var q = db.Products.AsNoTracking(); if (!string.IsNullOrWhiteSpace(Search)) q = q.Where(x => x.Name.Contains(Search) || x.Author.Contains(Search) || x.Isbn.Contains(Search)); if (!string.IsNullOrWhiteSpace(Category)) q = q.Where(x => x.Category == Category); Books = await q.ToListAsync(); }
    public async Task<IActionResult> OnPostAddAsync(int id) { if (!await db.Products.AnyAsync(x => x.Id == id)) return NotFound(); var cart = BrowserCart.Read(HttpContext.Session); cart[id] = Math.Min(cart.GetValueOrDefault(id) + 1, 999); BrowserCart.Write(HttpContext.Session, cart); return RedirectToPage("/Cart"); }
}

