using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace WebApplication1.Pages;

public class LogoutModel(SignInManager<ApplicationUser> signIn) : PageModel
{
    public async Task<IActionResult> OnPostAsync() { await signIn.SignOutAsync(); return RedirectToPage("/Index"); }
}

