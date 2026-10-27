using System.ComponentModel.DataAnnotations;
using WebApplication1;
namespace MvcBookstore.Models;

public class OrderEditModel
{
    public OrderEditInput Input { get; set; } = new();
    public int AddProductId { get; set; }
    public List<Product> Products { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<OrderLine> Snapshots { get; set; } = [];
    public OrderLine? Resolve(LineInput x) { if (x.OriginalLineId != 0) return Snapshots.FirstOrDefault(l => l.Id == x.OriginalLineId); var p = Products.FirstOrDefault(p => p.Id == x.ProductId); return p is null ? null : new() { ProductId = p.Id, ProductName = p.Name, UnitPrice = p.Price }; }
    public decimal Total => Input.Lines.Sum(x => (Resolve(x)?.UnitPrice ?? 0) * x.Quantity);
}
public class OrderEditInput
{
    [Range(0, int.MaxValue)] public int Id { get; set; }
    [Range(1, int.MaxValue)] public int CustomerId { get; set; }
    [Required, StringLength(300)] public string Address { get; set; } = "";
    [StringLength(500)] public string? Note { get; set; }
    [Required] public string Status { get; set; } = "Pending";
    public List<LineInput> Lines { get; set; } = [];
}
public class LineInput
{
    public int OriginalLineId { get; set; }
    public int? ProductId { get; set; }
    [Range(1, 999)] public int Quantity { get; set; } = 1;
}

