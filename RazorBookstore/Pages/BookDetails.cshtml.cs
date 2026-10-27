using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
namespace WebApplication1.Pages;

public class BookDetailsModel(AppDbContext db) : PageModel
{
    public Product Book { get; private set; } = null!;
    public async Task<IActionResult> OnGetAsync(int id) { var book = await db.Products.FindAsync(id); if (book is null) return NotFound(); Book = book; return Page(); }
    public async Task<IActionResult> OnPostAddAsync(int id) { if (await db.Products.FindAsync(id) is null) return NotFound(); var cart = BrowserCart.Read(HttpContext.Session); cart[id] = Math.Min(cart.GetValueOrDefault(id) + 1, 999); BrowserCart.Write(HttpContext.Session, cart); return RedirectToPage("/Cart"); }
}

