using System.ComponentModel.DataAnnotations;
namespace WebApplication1;

public sealed class CustomerInput
{
    [Range(0, int.MaxValue)] public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên khách hàng."), StringLength(120)] public string Name { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(200)] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại."), RegularExpression(@"\+?[0-9 ()\-]{8,20}", ErrorMessage = "Số điện thoại không hợp lệ.")] public string Phone { get; set; } = "";
    [StringLength(300)] public string? Address { get; set; } = "";
}
