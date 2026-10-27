using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace WebApplication1;
public class ApplicationUser : IdentityUser
{
    [Required, StringLength(120)] public string FullName { get; set; } = "";
    public int? CustomerId { get; set; }
    public bool IsDisabled { get; set; }
}
