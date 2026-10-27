using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Data;
using MvcBookstore.Models;
namespace MvcBookstore.Controllers;

[Route("Cart")]
public class CartController(AppDbContext db) : Controller
{
    [HttpGet("")] public async Task<IActionResult> Index() { var cart = BrowserCart.Read(HttpContext.Session); var ids = cart.Keys.ToArray(); return View(new CartModel { Lines = (await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync()).Select(x => new CartLine(x, cart[x.Id])).ToList() }); }
    [HttpPost("Add")] public async Task<IActionResult> Add(int id) { if (!await db.Products.AnyAsync(x => x.Id == id)) return NotFound(); var cart = BrowserCart.Read(HttpContext.Session); cart[id] = Math.Min(cart.GetValueOrDefault(id) + 1, 999); BrowserCart.Write(HttpContext.Session, cart); return RedirectToAction(nameof(Index)); }
    [HttpPost("Quantity")] public async Task<IActionResult> Quantity(int id, int quantity) { var cart = BrowserCart.Read(HttpContext.Session); if (quantity < 1) cart.Remove(id); else if (quantity <= 999 && await db.Products.AnyAsync(x => x.Id == id)) cart[id] = quantity; BrowserCart.Write(HttpContext.Session, cart); return RedirectToAction(nameof(Index)); }
    [HttpPost("Remove")] public IActionResult Remove(int id) { var cart = BrowserCart.Read(HttpContext.Session); cart.Remove(id); BrowserCart.Write(HttpContext.Session, cart); return RedirectToAction(nameof(Index)); }
    [HttpPost("Clear")] public IActionResult Clear() { HttpContext.Session.Remove("cart"); return RedirectToAction(nameof(Index)); }
}

