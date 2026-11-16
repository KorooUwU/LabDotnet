using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcBookstore.Models;
using WebApplication1;

namespace MvcBookstore.Controllers;

public partial class AdminController
{
    [Authorize(Policy = Permissions.Roles), HttpGet("Accounts")]
    public async Task<IActionResult> Accounts(string? search)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.UserName!.Contains(search) || x.FullName.Contains(search) || x.Email!.Contains(search));
        }
        var items = await query.OrderBy(x => x.UserName).ToListAsync();
        var ids = items.Select(x => x.Id).ToArray();
        var assignments = await (from ur in db.UserRoles
                                 join role in db.Roles on ur.RoleId equals role.Id
                                 where ids.Contains(ur.UserId)
                                 select new { ur.UserId, role.Name }).ToListAsync();
        return View(new AccountsModel
        {
            Search = search, Items = items,
            AccountRoles = assignments.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.Name!).ToArray())
        });
    }

    private async Task<AccountEditModel?> LoadAccountAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var account = await users.FindByIdAsync(id);
        if (account is null) return null;
        var claims = await db.RoleClaims.AsNoTracking().Where(x => x.ClaimType == "Permission").ToListAsync();
        return new()
        {
            Account = account,
            AvailableRoles = await db.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            RolePermissions = claims.GroupBy(x => x.RoleId).ToDictionary(g => g.Key, g => g.Select(x => x.ClaimValue!).ToArray()),
            RoleInput = new() { UserId = id, SelectedRoles = (await users.GetRolesAsync(account)).ToArray() },
            PasswordInput = new() { UserId = id }
        };
    }

    [Authorize(Policy = Permissions.Roles), HttpGet("AccountEdit")]
    public async Task<IActionResult> AccountEdit(string? id)
    {
        var model = await LoadAccountAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [Authorize(Policy = Permissions.Roles), HttpPost("AccountRoles")]
    public async Task<IActionResult> AccountRoles([Bind(Prefix = "RoleInput")] AccountRolesInput input)
    {
        var model = await LoadAccountAsync(input.UserId);
        if (model is null) return NotFound();
        input.SelectedRoles ??= [];
        model.RoleInput = input;
        if (input.SelectedRoles.Any(name => !model.AvailableRoles.Any(role => role.Name == name)))
            ModelState.AddModelError("", "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) return View("AccountEdit", model);

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var current = await users.GetRolesAsync(model.Account);
        if (current.Contains("Admin") && !input.SelectedRoles.Contains("Admin")
            && (await users.GetUsersInRoleAsync("Admin")).Count <= 1)
        {
            ModelState.AddModelError("", "Không được bỏ vai trò của quản trị viên cuối cùng.");
            return View("AccountEdit", model);
        }
        foreach (var result in new[]
        {
            await users.RemoveFromRolesAsync(model.Account, current.Except(input.SelectedRoles)),
            await users.AddToRolesAsync(model.Account, input.SelectedRoles.Except(current)),
            await users.UpdateSecurityStampAsync(model.Account)
        })
        {
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return View("AccountEdit", model);
            }
        }
        await transaction.CommitAsync();
        TempData["Message"] = "Đã cập nhật vai trò/quyền. Tài khoản vừa chỉnh sửa cần đăng nhập lại.";
        return User.FindFirstValue(ClaimTypes.NameIdentifier) == model.Account.Id
            ? Redirect("/Login")
            : RedirectToAction(nameof(AccountEdit), new { id = model.Account.Id });
    }

    [Authorize(Policy = Permissions.Roles), HttpPost("AccountPassword")]
    public async Task<IActionResult> AccountPassword([Bind(Prefix = "PasswordInput")] AdminPasswordInput input)
    {
        var model = await LoadAccountAsync(input.UserId);
        if (model is null) return NotFound();
        ModelState.SetModelValue("PasswordInput.NewPassword", "", "");
        ModelState.SetModelValue("PasswordInput.ConfirmPassword", "", "");
        if (!ModelState.IsValid) return View("AccountEdit", model);
        var token = await users.GeneratePasswordResetTokenAsync(model.Account);
        var result = await users.ResetPasswordAsync(model.Account, token, input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View("AccountEdit", model);
        }
        TempData["Message"] = "Đã đặt lại mật khẩu. Tài khoản vừa chỉnh sửa cần đăng nhập lại bằng mật khẩu mới.";
        return User.FindFirstValue(ClaimTypes.NameIdentifier) == model.Account.Id
            ? Redirect("/Login")
            : RedirectToAction(nameof(AccountEdit), new { id = model.Account.Id });
    }
}

