using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages;

public class CheckoutModel(AppDbContext db, UserManager<ApplicationUser> users) : PageModel
{
    [BindProperty] public CheckoutInput Input { get; set; } = new();
    public decimal Total { get; private set; }
    private async Task<List<OrderLine>> LoadLines() { var cart = BrowserCart.Read(HttpContext.Session); var ids = cart.Keys.ToArray(); var products = await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync(); return products.Select(x => new OrderLine { ProductId = x.Id, ProductName = x.Name, UnitPrice = x.Price, Quantity = cart[x.Id] }).ToList(); }
    public async Task<IActionResult> OnGetAsync() { var user = await users.GetUserAsync(User); if (user?.CustomerId is not int id) return Forbid(); Input.Address = (await db.Customers.FindAsync(id))?.Address ?? ""; Input.Token = Guid.NewGuid().ToString("N"); HttpContext.Session.SetString("checkout", Input.Token); Total = (await LoadLines()).Sum(x => x.UnitPrice * x.Quantity); return Page(); }
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await users.GetUserAsync(User); if (user?.CustomerId is not int customerId || user.IsDisabled) return Forbid();
        var previous = await db.Orders.FirstOrDefaultAsync(x => x.CheckoutToken == Input.Token && x.CustomerId == customerId); if (previous is not null) return RedirectToPage("/MyOrders", new { id = previous.Id });
        if (Input.Token != HttpContext.Session.GetString("checkout")) ModelState.AddModelError("", "Phiên thanh toán hết hạn. Hãy quay về giỏ hàng.");
        if (Input.PaymentMethod != "COD") ModelState.AddModelError("", "Chỉ hỗ trợ thanh toán khi nhận hàng (COD).");
        var cart = BrowserCart.Read(HttpContext.Session); var lines = await LoadLines(); Total = lines.Sum(x => x.UnitPrice * x.Quantity);
        if (lines.Count == 0 || lines.Count != cart.Count) ModelState.AddModelError("", "Giỏ hàng trống hoặc có sách đã ngừng bán. Vui lòng cập nhật lại giỏ hàng.");
        if (!ModelState.IsValid) return Page();
        var order = new Order { CustomerId = customerId, Address = Input.Address.Trim(), Note = Input.Note?.Trim() ?? "", PaymentMethod = "COD", CheckoutToken = Input.Token, Lines = lines };
        db.Orders.Add(order);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { var existing = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.CheckoutToken == Input.Token && x.CustomerId == customerId); if (existing is null) throw; order = existing; }
        HttpContext.Session.Remove("cart"); HttpContext.Session.Remove("checkout"); return RedirectToPage("/MyOrders", new { id = order.Id });
    }
}

