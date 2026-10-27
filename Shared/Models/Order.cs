namespace WebApplication1;

public sealed class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string PaymentMethod { get; set; } = "COD";
    public string? CheckoutToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string Address { get; set; } = "";
    public string Note { get; set; } = "";
    public List<OrderLine> Lines { get; set; } = [];
    public decimal Total => Lines.Sum(x => x.UnitPrice * x.Quantity);
}
public sealed class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    // Product name and price are snapshots; old orders survive product deletion.
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
public static class OrderStatuses
{
    public static IReadOnlyDictionary<string, string> All { get; } = new Dictionary<string, string>
    {
        ["Pending"] = "Chờ xác nhận",
        ["Confirmed"] = "Đã xác nhận",
        ["Shipping"] = "Đang giao",
        ["Completed"] = "Hoàn thành",
        ["Cancelled"] = "Đã hủy"
    };
}
public sealed class UserAccount
{
    public string UserName { get; set; } = "";
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Customer";
    public int? CustomerId { get; set; }
}


