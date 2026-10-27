using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Data;
using MvcBookstore.Models;
namespace MvcBookstore.Controllers;

[Authorize]
public class CheckoutController(AppDbContext db, UserManager<ApplicationUser> users) : Controller
{
    private async Task<List<OrderLine>> LoadLines() { var cart = BrowserCart.Read(HttpContext.Session); var ids = cart.Keys.ToArray(); return (await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync()).Select(x => new OrderLine { ProductId = x.Id, ProductName = x.Name, UnitPrice = x.Price, Quantity = cart[x.Id] }).ToList(); }
    [HttpGet("/Checkout")] public async Task<IActionResult> Index() { var user = await users.GetUserAsync(User); if (user?.CustomerId is not int id) return Forbid(); var token = Guid.NewGuid().ToString("N"); HttpContext.Session.SetString("checkout", token); return View(new CheckoutModel { Input = new() { Address = (await db.Customers.FindAsync(id))?.Address ?? "", Token = token }, Total = (await LoadLines()).Sum(x => x.UnitPrice * x.Quantity) }); }
    [HttpPost("/Checkout")]
    public async Task<IActionResult> Index(CheckoutInput input)
    {
        var user = await users.GetUserAsync(User); if (user?.CustomerId is not int customerId || user.IsDisabled) return Forbid(); var previous = await db.Orders.FirstOrDefaultAsync(x => x.CheckoutToken == input.Token && x.CustomerId == customerId); if (previous is not null) return Redirect("/MyOrders?id=" + previous.Id);
        if (input.Token != HttpContext.Session.GetString("checkout")) ModelState.AddModelError("", "Phiên thanh toán hết hạn. Hãy quay về giỏ hàng."); if (input.PaymentMethod != "COD") ModelState.AddModelError("", "Chỉ hỗ trợ COD."); var cart = BrowserCart.Read(HttpContext.Session); var lines = await LoadLines(); if (lines.Count == 0 || lines.Count != cart.Count) ModelState.AddModelError("", "Giỏ hàng trống hoặc có sách đã ngừng bán. Vui lòng cập nhật giỏ hàng."); if (!ModelState.IsValid) return View(new CheckoutModel { Input = input, Total = lines.Sum(x => x.UnitPrice * x.Quantity) });
        var order = new Order { CustomerId = customerId, Address = input.Address.Trim(), Note = input.Note?.Trim() ?? "", PaymentMethod = "COD", CheckoutToken = input.Token, Lines = lines }; db.Orders.Add(order);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { var existing = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.CheckoutToken == input.Token && x.CustomerId == customerId); if (existing is null) throw; order = existing; }
        HttpContext.Session.Remove("cart"); HttpContext.Session.Remove("checkout"); return Redirect("/MyOrders?id=" + order.Id);
    }
    [HttpGet("/MyOrders")] public async Task<IActionResult> MyOrders(int? id) { var user = await users.GetUserAsync(User); var q = db.Orders.AsNoTracking().Include(x => x.Lines).Where(x => x.CustomerId == user!.CustomerId); if (id.HasValue) q = q.Where(x => x.Id == id); return View(new MyOrdersModel { Orders = await q.OrderByDescending(x => x.Id).ToListAsync() }); }
}

