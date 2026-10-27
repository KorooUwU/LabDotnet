using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages;

public class RegisterModel(AppDbContext db, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public RegisterInput Input { get; set; } = new();
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Input.Password) || Input.Password.Length < 6) ModelState.AddModelError("", "Mật khẩu cần ít nhất 6 ký tự."); if (!ModelState.IsValid) return Page();
        if (await db.Customers.AnyAsync(x => x.Email == Input.Email)) { ModelState.AddModelError("", "Email đã có hồ sơ, vui lòng liên hệ quản trị."); return Page(); }
        await using var tx = await db.Database.BeginTransactionAsync();
        var customer = new Customer(0, Input.FullName.Trim(), Input.Email.Trim(), Input.Phone.Trim(), ""); db.Customers.Add(customer); await db.SaveChangesAsync();
        var user = new ApplicationUser { UserName = Input.UserName.Trim(), Email = Input.Email.Trim(), FullName = Input.FullName.Trim(), CustomerId = customer.Id };
        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return Page(); }
        result = await users.AddToRoleAsync(user, "Customer"); if (!result.Succeeded) throw new InvalidOperationException("Cannot assign Customer role");
        await tx.CommitAsync(); await signIn.SignInAsync(user, false); return RedirectToPage("/Index");
    }
}


