using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
namespace WebApplication1;
public sealed class IdentityRevalidation(ILoggerFactory logger,IServiceScopeFactory scopes):RevalidatingServerAuthenticationStateProvider(logger) {
 protected override TimeSpan RevalidationInterval=>TimeSpan.FromSeconds(30);
 protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState state,CancellationToken cancellationToken){
  await using var scope=scopes.CreateAsyncScope();var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();var user=await users.GetUserAsync(state.User);
  return user is not null&&!user.IsDisabled&&state.User.FindFirst("AspNet.Identity.SecurityStamp")?.Value==await users.GetSecurityStampAsync(user);
 }
}
