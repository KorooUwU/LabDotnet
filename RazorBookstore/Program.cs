using WebApplication1;
var builder = WebApplication.CreateBuilder(args);
builder.AddLab("Razor");
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", Permissions.Admin);
    options.Conventions.AuthorizePage("/Admin/Products", Permissions.Products);
    options.Conventions.AuthorizePage("/Admin/ProductEdit", Permissions.Products);
    options.Conventions.AuthorizePage("/Admin/Customers", Permissions.Customers);
    options.Conventions.AuthorizePage("/Admin/CustomerEdit", Permissions.Customers);
    options.Conventions.AuthorizePage("/Admin/Categories", Permissions.Categories);
    options.Conventions.AuthorizePage("/Admin/Orders", Permissions.Orders);
    options.Conventions.AuthorizePage("/Admin/OrderEdit", Permissions.Orders);
    options.Conventions.AuthorizePage("/Admin/Roles", Permissions.Roles);
    options.Conventions.AuthorizePage("/Checkout");
    options.Conventions.AuthorizePage("/MyOrders");
}).AddMvcOptions(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.Cookie.Name = "MocBooks.Razor.Cart"; options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; options.IdleTimeout = TimeSpan.FromHours(8); });
var app = builder.Build();
await app.InitializeLabAsync();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStatusCodePagesWithReExecute("/NotFound"); app.UseRouting(); app.UseSession(); app.UseAuthentication(); app.UseAuthorization();
app.MapStaticAssets(); app.MapRazorPages(); app.MapProductImages(); app.Run();

