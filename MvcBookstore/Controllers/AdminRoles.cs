using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using MvcBookstore.Models;
namespace MvcBookstore.Controllers;

public partial class AdminController
{
    private RolesModel RoleModel() => new() { Roles = db.Roles.AsNoTracking().ToList(), Accounts = db.Users.AsNoTracking().ToList(), RolePermissions = db.RoleClaims.AsNoTracking().Where(x => x.ClaimType == "Permission").ToList().GroupBy(x => x.RoleId).ToDictionary(g => g.Key, g => g.Select(x => x.ClaimValue!).ToArray()), UserRoles = db.UserRoles.AsNoTracking().ToList().GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.RoleId).ToArray()) };
    [Authorize(Policy = Permissions.Roles), HttpGet("Roles")] public IActionResult Roles() => View(RoleModel());
    [Authorize(Policy = Permissions.Roles), HttpPost("CreateRole")] public async Task<IActionResult> CreateRole(string name) { if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 50) ModelState.AddModelError("", "Tên vai trò từ 1 đến 50 ký tự."); if (!ModelState.IsValid) return View("Roles", RoleModel()); var result = await roles.CreateAsync(new(name.Trim())); if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return View("Roles", RoleModel()); } return RedirectToAction(nameof(Roles)); }
    [Authorize(Policy = Permissions.Roles), HttpPost("RolePermissions")] public async Task<IActionResult> RolePermissions(string roleId, string[] selected) { var role = await roles.FindByIdAsync(roleId); if (role is null) return NotFound(); if (role.Name == "Admin") { TempData["Message"] = "Vai trò Admin hệ thống luôn giữ đủ quyền."; return RedirectToAction(nameof(Roles)); } if (selected.Any(x => !Permissions.All.Contains(x))) return BadRequest(); await using var tx = await db.Database.BeginTransactionAsync(); foreach (var c in (await roles.GetClaimsAsync(role)).Where(x => x.Type == "Permission")) Check(await roles.RemoveClaimAsync(role, c)); foreach (var p in selected.Distinct()) Check(await roles.AddClaimAsync(role, new("Permission", p))); foreach (var user in await users.GetUsersInRoleAsync(role.Name!)) Check(await users.UpdateSecurityStampAsync(user)); await tx.CommitAsync(); return RedirectToAction(nameof(Roles)); }
    [Authorize(Policy = Permissions.Roles), HttpPost("UserRoles")] public async Task<IActionResult> UserRoles(string userId, string[] selectedRoles) { var user = await users.FindByIdAsync(userId); if (user is null) return NotFound(); var valid = await db.Roles.Select(x => x.Name!).ToArrayAsync(); if (selectedRoles.Any(x => !valid.Contains(x)) || selectedRoles.Length == 0) return BadRequest(); var current = await users.GetRolesAsync(user); if (current.Contains("Admin") && !selectedRoles.Contains("Admin") && (await users.GetUsersInRoleAsync("Admin")).Count <= 1) { TempData["Message"] = "Không được bỏ vai trò của quản trị viên cuối cùng."; return RedirectToAction(nameof(Roles)); } await using var tx = await db.Database.BeginTransactionAsync(); Check(await users.RemoveFromRolesAsync(user, current.Except(selectedRoles))); Check(await users.AddToRolesAsync(user, selectedRoles.Except(current))); Check(await users.UpdateSecurityStampAsync(user)); await tx.CommitAsync(); return RedirectToAction(nameof(Roles)); }
    private static void Check(IdentityResult result) { if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description))); }
}

