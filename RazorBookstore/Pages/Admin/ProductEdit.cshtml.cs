using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

[RequestSizeLimit(3 * 1024 * 1024)]
public class ProductEditModel(AppDbContext db, IWebHostEnvironment env) : PageModel
{
    [BindProperty] public ProductInput Input { get; set; } = new();
    [BindProperty] public IFormFile? Cover { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }
    public List<Category> Categories => db.Categories.AsNoTracking().ToList();
    public string? Preview { get; set; }
    public async Task<IActionResult> OnGetAsync(int id = 0) { if (id == 0) return Page(); var x = await db.Products.FindAsync(id); if (x is null) return NotFound(); Input = new() { Id = x.Id, Name = x.Name, Description = x.Description, Category = x.Category, CategoryId = x.CategoryId ?? 0, Price = x.Price, Author = x.Author, Publisher = x.Publisher, PublicationYear = x.PublicationYear, Isbn = x.Isbn, PageCount = x.PageCount, ColorClass = x.ColorClass }; Preview = x.ImageUrl; return Page(); }
    public async Task<IActionResult> OnPostAsync()
    {
        var old = Input.Id == 0 ? null : await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Input.Id); if (Input.Id != 0 && old is null) return NotFound(); Preview = old?.ImageUrl;
        var category = await db.Categories.FindAsync(Input.CategoryId); ModelState.Remove("Input.Category"); if (category is null) ModelState.AddModelError("", "Chọn danh mục hợp lệ.");
        if (!ModelState.IsValid) return Page(); string? uploaded = null;
        try
        {
            if (Cover is not null) uploaded = await ProductImages.SaveAsync(Cover, ProductImages.Root(env.ContentRootPath));
            var product = new Product(Input.Id, Input.Name.Trim(), Input.Description.Trim(), category!.Name, Input.Price, "", Input.ColorClass, uploaded ?? (RemoveImage ? null : old?.ImageUrl), Input.Author.Trim(), Input.Publisher?.Trim() ?? "", Input.PublicationYear, Input.Isbn?.Trim() ?? "", Input.PageCount) { CategoryId = category.Id };
            if (Input.Id == 0) db.Products.Add(product); else db.Products.Update(product);
            await db.SaveChangesAsync(); TempData["Message"] = "Đã lưu sách."; return RedirectToPage("Products");
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or DbUpdateException) { if (uploaded is not null) System.IO.File.Delete(Path.Combine(ProductImages.Root(env.ContentRootPath), Path.GetFileName(uploaded))); ModelState.AddModelError("", ex is InvalidDataException ? ex.Message : "Không thể lưu sách. Hãy thử lại."); return Page(); }
    }
}

