using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class CategoriesModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty] public Category Input { get; set; } = new();
    public List<Category> Items => db.Categories.AsNoTracking().Where(x => Search == null || x.Name.Contains(Search)).ToList();
    public async Task<IActionResult> OnGetAsync(int id = 0) { if (id != 0) { var x = await db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id); if (x is null) return NotFound(); Input = x; } return Page(); }
    public async Task<IActionResult> OnPostSaveAsync() { if (!ModelState.IsValid) return Page(); Input.Name = Input.Name.Trim(); Input.Description ??= ""; if (await db.Categories.AnyAsync(x => x.Id != Input.Id && x.Name == Input.Name)) { ModelState.AddModelError("", "Tên danh mục đã tồn tại."); return Page(); } if (Input.Id == 0) db.Categories.Add(Input); else { var old = await db.Categories.FindAsync(Input.Id); if (old is null) return NotFound(); old.Name = Input.Name; old.Description = Input.Description ?? ""; foreach (var p in await db.Products.Where(x => x.CategoryId == old.Id).ToListAsync()) db.Entry(p).Property(x => x.Category).CurrentValue = old.Name; } await db.SaveChangesAsync(); TempData["Message"] = "Đã lưu danh mục."; return RedirectToPage(); }
    public async Task<IActionResult> OnPostDeleteAsync(int id) { if (await db.Products.AnyAsync(x => x.CategoryId == id)) { TempData["Message"] = "Danh mục đang có sách. Chuyển sách sang danh mục khác trước khi xóa."; return RedirectToPage(); } var x = await db.Categories.FindAsync(id); if (x is null) return NotFound(); db.Categories.Remove(x); await db.SaveChangesAsync(); return RedirectToPage(); }
}


