using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1;
public static class AccountEndpoints {
 public static void MapAccounts(this WebApplication app){
  app.MapPost("/account/login",async(HttpContext http,IAntiforgery antiforgery,UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn)=>{
   await antiforgery.ValidateRequestAsync(http);var form=await http.Request.ReadFormAsync();var name=form["username"].ToString().Trim();var password=form["password"].ToString();if(name.Length>50||password.Length>200)return Results.LocalRedirect("/Login?error=invalid");
   var user=await users.FindByNameAsync(name);if(user is null||user.IsDisabled||!(await signIn.PasswordSignInAsync(user,password,false,true)).Succeeded)return Results.LocalRedirect("/Login?error=invalid");
   var principal=await signIn.CreateUserPrincipalAsync(user);return Results.LocalRedirect(principal.HasClaim("Permission",Permissions.Admin)?"/Admin":"/");
  });
  app.MapPost("/account/register",async(HttpContext http,IAntiforgery antiforgery,AppDbContext db,UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn)=>{
   await antiforgery.ValidateRequestAsync(http);var f=await http.Request.ReadFormAsync();var input=new RegisterInput{UserName=f["username"].ToString().Trim(),Password=f["password"].ToString(),ConfirmPassword=f["confirm"].ToString(),FullName=f["fullname"].ToString().Trim(),Email=f["email"].ToString().Trim(),Phone=f["phone"].ToString().Trim()};
   if(!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input,new(input),new List<System.ComponentModel.DataAnnotations.ValidationResult>(),true)||input.Password.Length<6||await db.Customers.AnyAsync(x=>x.Email==input.Email))return Results.LocalRedirect("/Register?error=invalid");
   await using var tx=await db.Database.BeginTransactionAsync();var customer=new Customer(0,input.FullName,input.Email,input.Phone,"");db.Customers.Add(customer);await db.SaveChangesAsync();var user=new ApplicationUser{UserName=input.UserName,Email=input.Email,FullName=input.FullName,CustomerId=customer.Id};var result=await users.CreateAsync(user,input.Password);if(!result.Succeeded)return Results.LocalRedirect("/Register?error=invalid");result=await users.AddToRoleAsync(user,"Customer");if(!result.Succeeded)throw new InvalidOperationException("Cannot assign Customer role");await tx.CommitAsync();await signIn.SignInAsync(user,false);return Results.LocalRedirect("/");
  });
  app.MapPost("/account/logout",async(HttpContext http,IAntiforgery antiforgery,SignInManager<ApplicationUser> signIn)=>{await antiforgery.ValidateRequestAsync(http);await signIn.SignOutAsync();return Results.LocalRedirect("/");});
 }
}
