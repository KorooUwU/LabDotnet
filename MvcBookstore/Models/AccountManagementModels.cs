using Microsoft.AspNetCore.Identity;
using WebApplication1;

namespace MvcBookstore.Models;

public sealed class AccountsModel
{
    public string? Search { get; set; }
    public List<ApplicationUser> Items { get; set; } = [];
    public Dictionary<string, string[]> AccountRoles { get; set; } = [];
}

public sealed class AccountEditModel
{
    public ApplicationUser Account { get; set; } = null!;
    public List<IdentityRole> AvailableRoles { get; set; } = [];
    public Dictionary<string, string[]> RolePermissions { get; set; } = [];
    public AccountRolesInput RoleInput { get; set; } = new();
    public AdminPasswordInput PasswordInput { get; set; } = new();
}

