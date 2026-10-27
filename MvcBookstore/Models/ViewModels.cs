using Microsoft.AspNetCore.Identity;
using WebApplication1;
namespace MvcBookstore.Models;

public class FormModel<T> where T : new() { public T Input { get; set; } = new(); }
public class HomeModel { public string? Search { get; set; } public string? Category { get; set; } public List<Product> Books { get; set; } = []; public List<Category> Categories { get; set; } = []; }
public class BookModel { public Product Book { get; set; } = null!; }
public class CartModel { public List<CartLine> Lines { get; set; } = []; public int Count => Lines.Sum(x => x.Quantity); public decimal Total => Lines.Sum(x => x.Total); }
public record CartLine(Product Product, int Quantity) { public decimal Total => Product.Price * Quantity; }
public class CheckoutModel : FormModel<CheckoutInput> { public decimal Total { get; set; } }
public class MyOrdersModel { public List<Order> Orders { get; set; } = []; }
public class DashboardModel { public int Products { get; set; } public int Customers { get; set; } public int Categories { get; set; } public int Orders { get; set; } public decimal Revenue { get; set; } }
public class ProductsModel { public string? Search { get; set; } public List<Product> Items { get; set; } = []; }
public class CustomersModel { public string? Search { get; set; } public List<Customer> Items { get; set; } = []; }
public class CategoriesModel : FormModel<Category> { public string? Search { get; set; } public List<Category> Items { get; set; } = []; }
public class ProductEditModel : FormModel<ProductInput> { public IFormFile? Cover { get; set; } public bool RemoveImage { get; set; } public string? Preview { get; set; } public List<Category> Categories { get; set; } = []; }
public class CustomerEditModel : FormModel<CustomerInput> { }
public class OrdersModel { public string? Search { get; set; } public string? Status { get; set; } public List<Order> Items { get; set; } = []; }
public class RolesModel { public List<IdentityRole> Roles { get; set; } = []; public List<ApplicationUser> Accounts { get; set; } = []; public Dictionary<string, string[]> RolePermissions { get; set; } = []; public Dictionary<string, string[]> UserRoles { get; set; } = []; }

