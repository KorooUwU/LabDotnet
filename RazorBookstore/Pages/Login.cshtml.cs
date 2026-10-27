using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication1.Pages;

[AllowAnonymous]
public class LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    public async Task<IActionResult> OnPostAsync() { if (!ModelState.IsValid) return Page(); var user = await users.FindByNameAsync(Input.UserName.Trim()); if (user is null || user.IsDisabled) { ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng."); return Page(); } var result = await signIn.PasswordSignInAsync(user, Input.Password, false, true); if (!result.Succeeded) { ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản tạm khóa."); return Page(); } var principal = await signIn.CreateUserPrincipalAsync(user); return principal.HasClaim("Permission", Permissions.Admin) ? RedirectToPage("/Admin/Index") : RedirectToPage("/Index"); }
}

