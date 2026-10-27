using System.ComponentModel.DataAnnotations;
namespace WebApplication1;

public class LoginInput
{
    [Required, StringLength(50)] public string UserName { get; set; } = "";
    [Required, StringLength(200), DataType(DataType.Password)] public string Password { get; set; } = "";
}
public class RegisterInput : LoginInput
{
    [Required, StringLength(120)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"\+?[0-9 ()\-]{8,20}")] public string Phone { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public class CheckoutInput
{
    [Required, StringLength(300)] public string Address { get; set; } = "";
    [StringLength(500)] public string? Note { get; set; }
    [Required] public string Token { get; set; } = "";
    public string PaymentMethod { get; set; } = "COD";
}

