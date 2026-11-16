using System.ComponentModel.DataAnnotations;

namespace WebApplication1;
public sealed class AccountRolesInput
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = "";

    [Required, MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một vai trò.")]
    public string[] SelectedRoles { get; set; } = [];
}

public sealed class AdminPasswordInput
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [StringLength(200, MinimumLength = 6, ErrorMessage = "Mật khẩu cần từ 6 đến 200 ký tự.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Hai mật khẩu không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}
