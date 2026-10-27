using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
namespace WebApplication1;

public sealed class IdentityClaims(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, IOptions<IdentityOptions> options) : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(users, roles, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user) { var identity = await base.GenerateClaimsAsync(user); identity.AddClaim(new("FullName", user.FullName)); if (user.CustomerId is int id) identity.AddClaim(new("CustomerId", id.ToString())); return identity; }
}
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
public sealed class DatabasePermissionHandler(IServiceScopeFactory scopes) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier); if (id is null) return;
        await using var scope = scopes.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!await db.Users.AnyAsync(x => x.Id == id && !x.IsDisabled)) return;
        if (await (from ur in db.UserRoles join rc in db.RoleClaims on ur.RoleId equals rc.RoleId where ur.UserId == id && rc.ClaimType == "Permission" && rc.ClaimValue == requirement.Permission select rc.Id).AnyAsync()) context.Succeed(requirement);
    }
}

