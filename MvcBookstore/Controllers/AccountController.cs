using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Data;
using MvcBookstore.Models;
namespace MvcBookstore.Controllers;

public class AccountController(AppDbContext db, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet("/Login")] public IActionResult Login() => View(new FormModel<LoginInput>());
    [HttpPost("/Login")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if (!ModelState.IsValid) return View(new FormModel<LoginInput> { Input = input }); var user = await users.FindByNameAsync(input.UserName.Trim()); if (user is null || user.IsDisabled || !(await signIn.PasswordSignInAsync(user, input.Password, false, true)).Succeeded) { ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản tạm khóa."); return View(new FormModel<LoginInput> { Input = input }); }
        var principal = await signIn.CreateUserPrincipalAsync(user); return Redirect(principal.HasClaim("Permission", Permissions.Admin) ? "/Admin" : "/");
    }
    [HttpGet("/Register")] public IActionResult Register() => View(new FormModel<RegisterInput>());
    [HttpPost("/Register")]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        if (string.IsNullOrEmpty(input.Password) || input.Password.Length < 6) ModelState.AddModelError("", "Mật khẩu cần ít nhất 6 ký tự."); if (!ModelState.IsValid) return View(new FormModel<RegisterInput> { Input = input }); if (await db.Customers.AnyAsync(x => x.Email == input.Email)) { ModelState.AddModelError("", "Email đã có hồ sơ, vui lòng liên hệ quản trị."); return View(new FormModel<RegisterInput> { Input = input }); }
        await using var tx = await db.Database.BeginTransactionAsync(); var customer = new Customer(0, input.FullName.Trim(), input.Email.Trim(), input.Phone.Trim(), ""); db.Customers.Add(customer); await db.SaveChangesAsync(); var user = new ApplicationUser { UserName = input.UserName.Trim(), Email = input.Email.Trim(), FullName = input.FullName.Trim(), CustomerId = customer.Id }; var result = await users.CreateAsync(user, input.Password); if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return View(new FormModel<RegisterInput> { Input = input }); }
        result = await users.AddToRoleAsync(user, "Customer"); if (!result.Succeeded) throw new InvalidOperationException("Cannot assign Customer role"); await tx.CommitAsync(); await signIn.SignInAsync(user, false); return Redirect("/");
    }
    [HttpPost("/Logout")] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return Redirect("/"); }
}


