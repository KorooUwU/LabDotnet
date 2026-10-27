using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace WebApplication1.Pages.Admin;

public class OrderEditModel(WebApplication1.Data.AppDbContext db) : PageModel
{
    [BindProperty] public OrderInput Input { get; set; } = new();
    [BindProperty] public int AddProductId { get; set; }
    public IReadOnlyList<Customer> Customers => db.Customers.AsNoTracking().ToArray();
    public IReadOnlyList<Product> Products => db.Products.AsNoTracking().ToArray();
    private Order? Original => Input.Id == 0 ? null : db.Orders.AsNoTracking().Include(x => x.Customer).Include(x => x.Lines).ToArray().FirstOrDefault(x => x.Id == Input.Id);
    public OrderLine? Resolve(LineInput line) { if (line.OriginalLineId != 0) return Original?.Lines.FirstOrDefault(x => x.Id == line.OriginalLineId); var book = Products.FirstOrDefault(x => x.Id == line.ProductId); return book is null ? null : new() { ProductId = book.Id, ProductName = book.Name, UnitPrice = book.Price, Quantity = line.Quantity }; }
    public decimal Total => Input.Lines.Sum(x => (Resolve(x)?.UnitPrice ?? 0) * x.Quantity);
    public IActionResult OnGet(int id = 0) { if (id == 0) return Page(); var o = db.Orders.AsNoTracking().Include(x => x.Customer).Include(x => x.Lines).ToArray().FirstOrDefault(x => x.Id == id); if (o is null) return NotFound(); Input = new() { Id = o.Id, CustomerId = o.CustomerId, Address = o.Address, Note = o.Note, Status = o.Status, Lines = o.Lines.Select(x => new LineInput { OriginalLineId = x.Id, ProductId = x.ProductId, Quantity = x.Quantity }).ToList() }; return Page(); }
    public IActionResult OnPostAdd() { if (Input.Id != 0 && Original is null) return NotFound(); ModelState.Clear(); if (!Products.Any(x => x.Id == AddProductId)) { ModelState.AddModelError("", "Vui lòng chọn sách."); return Page(); } if (Input.Lines.Count >= 100) { ModelState.AddModelError("", "Tối đa 100 dòng trong một đơn."); return Page(); } Input.Lines.Add(new() { ProductId = AddProductId, Quantity = 1 }); return Page(); }
    public IActionResult OnPostRemove(int index) { if (Input.Id != 0 && Original is null) return NotFound(); if (index >= 0 && index < Input.Lines.Count) Input.Lines.RemoveAt(index); ModelState.Clear(); return Page(); }
    public IActionResult OnPostSave() { if (Input.Id != 0 && Original is null) return NotFound(); if (!OrderStatuses.All.ContainsKey(Input.Status)) ModelState.AddModelError("", "Trạng thái không hợp lệ."); if (Input.Lines.Count == 0 || Input.Lines.Count > 100) ModelState.AddModelError("", "Đơn hàng cần từ 1 đến 100 dòng sách."); var lines = new List<OrderLine>(); foreach (var item in Input.Lines) { var line = Resolve(item); if (line is null) { ModelState.AddModelError("", "Sách không còn tồn tại. Hãy xóa dòng đó hoặc chọn sách khác."); continue; } lines.Add(new() { ProductId = line.ProductId, ProductName = line.ProductName, UnitPrice = line.UnitPrice, Quantity = item.Quantity }); } if (!ModelState.IsValid) return Page(); try { if (SaveOrder(new() { Id = Input.Id, CustomerId = Input.CustomerId, Status = Input.Status, Address = Input.Address.Trim(), Note = Input.Note?.Trim() ?? "", Lines = lines })) { TempData["Message"] = "Đã lưu đơn hàng."; return RedirectToPage("Orders"); } ModelState.AddModelError("", "Không thể lưu đơn. Kiểm tra khách hàng và số lượng."); } catch (DbUpdateException) { ModelState.AddModelError("", "Không thể lưu đơn vào database."); } return Page(); }
    public sealed class OrderInput
    {
        [Range(0, int.MaxValue)] public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khách hàng.")] public int CustomerId { get; set; }
        [Required, StringLength(300)] public string Address { get; set; } = "";
        [StringLength(500)] public string? Note { get; set; } = "";
        [Required] public string Status { get; set; } = "Pending";
        public List<LineInput> Lines { get; set; } = [];
    }
    public sealed class LineInput
    {
        public int OriginalLineId { get; set; }
        public int? ProductId { get; set; }
        [Range(1, 999)] public int Quantity { get; set; } = 1;
    }
    private bool SaveOrder(Order order)
    {
        if (!db.Customers.Any(x => x.Id == order.CustomerId)) return false;
        if (order.Id == 0) db.Orders.Add(order);
        else { var old = db.Orders.Include(x => x.Lines).FirstOrDefault(x => x.Id == order.Id); if (old is null) return false; old.CustomerId = order.CustomerId; old.Address = order.Address; old.Note = order.Note; old.Status = order.Status; db.RemoveRange(old.Lines); old.Lines = order.Lines; }
        db.SaveChanges(); return true;
    }
}
