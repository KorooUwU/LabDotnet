using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class RolesModel(AppDbContext db, RoleManager<IdentityRole> roles, UserManager<ApplicationUser> users) : PageModel
{
    public List<IdentityRole> Roles => db.Roles.AsNoTracking().ToList();
    public List<ApplicationUser> Accounts => db.Users.AsNoTracking().ToList();
    public Dictionary<string, string[]> RolePermissions => db.RoleClaims.AsNoTracking().Where(x => x.ClaimType == "Permission").ToList().GroupBy(x => x.RoleId).ToDictionary(g => g.Key, g => g.Select(x => x.ClaimValue!).ToArray());
    public Dictionary<string, string[]> UserRoles => db.UserRoles.AsNoTracking().ToList().GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.RoleId).ToArray());
    public async Task<IActionResult> OnPostCreateAsync(string name) { if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 50) { ModelState.AddModelError("", "Tên vai trò từ 1 đến 50 ký tự."); return Page(); } var result = await roles.CreateAsync(new(name.Trim())); if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return Page(); } return RedirectToPage(); }
    public async Task<IActionResult> OnPostPermissionsAsync(string roleId, string[] selected)
    {
        var role = await roles.FindByIdAsync(roleId); if (role is null) return NotFound(); if (role.Name == "Admin") { TempData["Message"] = "Vai trò Admin hệ thống luôn giữ đủ quyền."; return RedirectToPage(); }
        if (selected.Any(x => !Permissions.All.Contains(x))) return BadRequest(); var before = await roles.GetClaimsAsync(role);
        await using var tx = await db.Database.BeginTransactionAsync();
        foreach (var c in before.Where(x => x.Type == "Permission")) Check(await roles.RemoveClaimAsync(role, c));
        foreach (var p in selected.Distinct()) Check(await roles.AddClaimAsync(role, new("Permission", p)));
        foreach (var user in await users.GetUsersInRoleAsync(role.Name!)) Check(await users.UpdateSecurityStampAsync(user));
        await tx.CommitAsync(); return RedirectToPage();
    }
    public async Task<IActionResult> OnPostUserAsync(string userId, string[] selectedRoles)
    {
        var user = await users.FindByIdAsync(userId); if (user is null) return NotFound(); var valid = Roles.Select(x => x.Name!).ToArray(); if (selectedRoles.Any(x => !valid.Contains(x)) || selectedRoles.Length == 0) return BadRequest();
        var current = await users.GetRolesAsync(user); if (current.Contains("Admin") && !selectedRoles.Contains("Admin") && (await users.GetUsersInRoleAsync("Admin")).Count <= 1) { TempData["Message"] = "Không được bỏ vai trò của quản trị viên cuối cùng."; return RedirectToPage(); }
        await using var tx = await db.Database.BeginTransactionAsync(); Check(await users.RemoveFromRolesAsync(user, current.Except(selectedRoles))); Check(await users.AddToRolesAsync(user, selectedRoles.Except(current))); Check(await users.UpdateSecurityStampAsync(user)); await tx.CommitAsync(); return RedirectToPage();
    }
    private static void Check(IdentityResult result) { if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description))); }
}

