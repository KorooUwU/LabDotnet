using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages;

public class CartModel(AppDbContext db) : PageModel
{
    public List<(Product Product, int Quantity, decimal Total)> Lines { get; private set; } = [];
    public int Count => Lines.Sum(x => x.Quantity);
    public decimal Total => Lines.Sum(x => x.Total);
    public async Task OnGetAsync() { var cart = BrowserCart.Read(HttpContext.Session); var ids = cart.Keys.ToArray(); Lines = (await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync()).Select(x => (x, cart[x.Id], x.Price * cart[x.Id])).ToList(); }
    public async Task<IActionResult> OnPostQuantityAsync(int id, int quantity) { var cart = BrowserCart.Read(HttpContext.Session); if (quantity < 1) cart.Remove(id); else if (quantity <= 999 && await db.Products.AnyAsync(x => x.Id == id)) cart[id] = quantity; BrowserCart.Write(HttpContext.Session, cart); return RedirectToPage(); }
    public IActionResult OnPostRemove(int id) { var cart = BrowserCart.Read(HttpContext.Session); cart.Remove(id); BrowserCart.Write(HttpContext.Session, cart); return RedirectToPage(); }
    public IActionResult OnPostClear() { HttpContext.Session.Remove("cart"); return RedirectToPage(); }
}

