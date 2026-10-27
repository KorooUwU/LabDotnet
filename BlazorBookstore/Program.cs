using WebApplication1;
using WebApplication1.Components;
using WebApplication1.Data;
var builder=WebApplication.CreateBuilder(args);
builder.AddLab("Blazor");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider,WebApplication1.IdentityRevalidation>();
builder.Services.AddScoped<CartStore>();builder.Services.AddScoped<AdminFeedback>();builder.Services.AddScoped<NotificationStore>();
builder.Services.AddSingleton<ProductImageStore>();
var app=builder.Build();await app.InitializeLabAsync();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Error",createScopeForErrors:true);app.UseHsts();}
app.UseHttpsRedirection();app.UseAuthentication();app.UseAuthorization();app.UseAntiforgery();app.MapStaticAssets();app.MapAccounts();app.MapProductImages();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();app.Run();
