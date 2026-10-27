using System.ComponentModel.DataAnnotations;
namespace WebApplication1;

public sealed class ProductInput
{
    [Range(0, int.MaxValue)] public int Id { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên sách."), StringLength(120)] public string Name { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mô tả."), StringLength(500)] public string Description { get; set; } = "";
    [StringLength(80)] public string Category { get; set; } = "";
    [Range(typeof(decimal), "1", "1000000000", ErrorMessage = "Giá phải từ 1 đến 1.000.000.000đ.")] public decimal Price { get; set; }
    public string Emoji { get; set; } = "";
    public string? ImageUrl { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tác giả."), StringLength(120)] public string Author { get; set; } = "";
    [StringLength(120)] public string Publisher { get; set; } = "";
    [Range(0, 2100)] public int PublicationYear { get; set; }
    [RegularExpression(@"^(?:[0-9]{10}|[0-9]{13})?$", ErrorMessage = "ISBN gồm 10 hoặc 13 chữ số, có thể để trống.")] public string Isbn { get; set; } = "";
    [Range(0, 10000)] public int PageCount { get; set; }
    [Required, RegularExpression("^(clay|amber|sage|terracotta)$")] public string ColorClass { get; set; } = "clay";
}





