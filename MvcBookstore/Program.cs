using WebApplication1;
var builder = WebApplication.CreateBuilder(args); builder.AddLab("Mvc");
builder.Services.AddControllersWithViews(options => { options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()); });
builder.Services.AddDistributedMemoryCache(); builder.Services.AddSession(options => { options.Cookie.Name = "MocBooks.Mvc.Cart"; options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; options.IdleTimeout = TimeSpan.FromHours(8); });
var app = builder.Build(); await app.InitializeLabAsync();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseWhen(context => HttpMethods.IsGet(context.Request.Method), branch => branch.UseStatusCodePagesWithReExecute("/NotFound")); app.UseRouting(); app.UseSession(); app.UseAuthentication(); app.UseAuthorization(); app.MapStaticAssets(); app.MapControllers(); app.MapProductImages(); app.Run();

