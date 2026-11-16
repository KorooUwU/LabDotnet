using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin;

public class AccountEditModel(AppDbContext db, UserManager<ApplicationUser> users) : PageModel
{
    public ApplicationUser Account { get; private set; } = null!;
    public List<IdentityRole> AvailableRoles { get; private set; } = [];
    public Dictionary<string, string[]> RolePermissions { get; private set; } = [];
    public AccountRolesInput RoleInput { get; set; } = new();
    public AdminPasswordInput PasswordInput { get; set; } = new();

    private async Task<bool> LoadAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        var account = await users.FindByIdAsync(id);
        if (account is null) return false;
        Account = account;
        AvailableRoles = await db.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        var claims = await db.RoleClaims.AsNoTracking().Where(x => x.ClaimType == "Permission").ToListAsync();
        RolePermissions = claims.GroupBy(x => x.RoleId).ToDictionary(g => g.Key, g => g.Select(x => x.ClaimValue!).ToArray());
        RoleInput = new() { UserId = id, SelectedRoles = (await users.GetRolesAsync(account)).ToArray() };
        PasswordInput = new() { UserId = id };
        return true;
    }

    public async Task<IActionResult> OnGetAsync(string? id) => await LoadAsync(id) ? Page() : NotFound();

    public async Task<IActionResult> OnPostRolesAsync([Bind(Prefix = "RoleInput")] AccountRolesInput input)
    {
        if (!await LoadAsync(input.UserId)) return NotFound();
        input.SelectedRoles ??= [];
        RoleInput = input;
        if (input.SelectedRoles.Any(name => !AvailableRoles.Any(role => role.Name == name)))
            ModelState.AddModelError("", "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) return Page();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var current = await users.GetRolesAsync(Account);
        if (current.Contains("Admin") && !input.SelectedRoles.Contains("Admin")
            && (await users.GetUsersInRoleAsync("Admin")).Count <= 1)
        {
            ModelState.AddModelError("", "Không được bỏ vai trò của quản trị viên cuối cùng.");
            return Page();
        }
        foreach (var result in new[]
        {
            await users.RemoveFromRolesAsync(Account, current.Except(input.SelectedRoles)),
            await users.AddToRolesAsync(Account, input.SelectedRoles.Except(current)),
            await users.UpdateSecurityStampAsync(Account)
        })
        {
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return Page();
            }
        }
        await transaction.CommitAsync();
        TempData["Message"] = "Đã cập nhật vai trò/quyền. Tài khoản vừa chỉnh sửa cần đăng nhập lại.";
        return User.FindFirstValue(ClaimTypes.NameIdentifier) == Account.Id
            ? RedirectToPage("/Login")
            : RedirectToPage(new { id = Account.Id });
    }

    public async Task<IActionResult> OnPostPasswordAsync([Bind(Prefix = "PasswordInput")] AdminPasswordInput input)
    {
        if (!await LoadAsync(input.UserId)) return NotFound();
        // Keep validation errors but never render submitted passwords back into HTML.
        ModelState.SetModelValue("PasswordInput.NewPassword", "", "");
        ModelState.SetModelValue("PasswordInput.ConfirmPassword", "", "");
        if (!ModelState.IsValid) return Page();
        var token = await users.GeneratePasswordResetTokenAsync(Account);
        var result = await users.ResetPasswordAsync(Account, token, input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }
        TempData["Message"] = "Đã đặt lại mật khẩu. Tài khoản vừa chỉnh sửa cần đăng nhập lại bằng mật khẩu mới.";
        return User.FindFirstValue(ClaimTypes.NameIdentifier) == Account.Id
            ? RedirectToPage("/Login")
            : RedirectToPage(new { id = Account.Id });
    }
}

