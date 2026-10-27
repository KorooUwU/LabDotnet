using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using MvcBookstore.Models;
namespace MvcBookstore.Controllers;

public partial class AdminController
{
    private async Task<bool> LoadOrderModel(OrderEditModel model) { model.Products = await db.Products.AsNoTracking().ToListAsync(); model.Customers = await db.Customers.AsNoTracking().ToListAsync(); if (model.Input.Id != 0) { var old = await db.Orders.AsNoTracking().Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == model.Input.Id); if (old is null) return false; model.Snapshots = old.Lines; } return true; }
    [Authorize(Policy = Permissions.Orders), HttpGet("OrderEdit")] public async Task<IActionResult> OrderEdit(int id = 0) { var model = new OrderEditModel(); if (id != 0) { var o = await db.Orders.AsNoTracking().Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id); if (o is null) return NotFound(); model.Input = new() { Id = o.Id, CustomerId = o.CustomerId, Address = o.Address, Note = o.Note, Status = o.Status, Lines = o.Lines.Select(x => new LineInput { OriginalLineId = x.Id, ProductId = x.ProductId, Quantity = x.Quantity }).ToList() }; } await LoadOrderModel(model); return View(model); }
    [Authorize(Policy = Permissions.Orders), HttpPost("OrderAdd")] public async Task<IActionResult> OrderAdd(OrderEditModel model) { if (!await LoadOrderModel(model)) return NotFound(); ModelState.Clear(); if (!model.Products.Any(x => x.Id == model.AddProductId) || model.Input.Lines.Count >= 100) ModelState.AddModelError("", "Chọn sách hợp lệ; tối đa 100 dòng."); else model.Input.Lines.Add(new() { ProductId = model.AddProductId }); return View("OrderEdit", model); }
    [Authorize(Policy = Permissions.Orders), HttpPost("OrderRemove")] public async Task<IActionResult> OrderRemove(OrderEditModel model, int index) { if (!await LoadOrderModel(model)) return NotFound(); if (index >= 0 && index < model.Input.Lines.Count) model.Input.Lines.RemoveAt(index); ModelState.Clear(); return View("OrderEdit", model); }
    [Authorize(Policy = Permissions.Orders), HttpPost("OrderSave")]
    public async Task<IActionResult> OrderSave(OrderEditModel model)
    {
        if (!await LoadOrderModel(model)) return NotFound(); var input = model.Input; if (!OrderStatuses.All.ContainsKey(input.Status)) ModelState.AddModelError("", "Trạng thái không hợp lệ."); if (input.Lines.Count == 0 || input.Lines.Count > 100) ModelState.AddModelError("", "Đơn cần từ 1 đến 100 dòng sách."); if (!await db.Customers.AnyAsync(x => x.Id == input.CustomerId)) ModelState.AddModelError("", "Khách hàng không tồn tại.");
        var lines = new List<OrderLine>(); foreach (var x in input.Lines) { var snapshot = model.Resolve(x); if (snapshot is null) ModelState.AddModelError("", "Sách không còn tồn tại."); else lines.Add(new() { ProductId = snapshot.ProductId, ProductName = snapshot.ProductName, UnitPrice = snapshot.UnitPrice, Quantity = x.Quantity }); }
        if (!ModelState.IsValid) return View("OrderEdit", model);
        if (input.Id == 0) db.Orders.Add(new() { CustomerId = input.CustomerId, Address = input.Address.Trim(), Note = input.Note?.Trim() ?? "", Status = input.Status, Lines = lines });
        else { var old = await db.Orders.Include(x => x.Lines).FirstAsync(x => x.Id == input.Id); old.CustomerId = input.CustomerId; old.Address = input.Address.Trim(); old.Note = input.Note?.Trim() ?? ""; old.Status = input.Status; db.RemoveRange(old.Lines); old.Lines = lines; }
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Orders));
    }
}

