using System.ComponentModel.DataAnnotations;
namespace WebApplication1;
public class Category
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [StringLength(500)] public string Description { get; set; } = "";
}
