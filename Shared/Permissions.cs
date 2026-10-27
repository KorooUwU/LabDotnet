namespace WebApplication1;
public static class Permissions
{
    public const string Admin = "Admin.Access";
    public const string Products = "Products.Manage";
    public const string Customers = "Customers.Manage";
    public const string Categories = "Categories.Manage";
    public const string Orders = "Orders.Manage";
    public const string Roles = "Roles.Manage";
    public static readonly string[] All = [Admin, Products, Customers, Categories, Orders, Roles];
}
