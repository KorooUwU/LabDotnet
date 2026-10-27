using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace WebApplication1.Pages;

public class NotificationsModel : PageModel
{
    public static IReadOnlyList<Notice> All { get; } = [new(1, "Chào mừng đến với Mộc Books", "Khám phá sách văn học, thiếu nhi, công nghệ và phát triển bản thân.", "/"), new(2, "Khám phá bộ sưu tập", "Tìm sách theo tên, tác giả hoặc thể loại.", "/#products"), new(3, "Cần hỗ trợ mua hàng?", "Xem hướng dẫn mua hàng và câu hỏi thường gặp.", "/Support")];
    public bool IsRead(int id) => HttpContext.Session.GetInt32("notice:" + id) == 1;
    public int UnreadCount => All.Count(x => !IsRead(x.Id));
    public IActionResult OnPostRead(int id) { if (All.Any(x => x.Id == id)) HttpContext.Session.SetInt32("notice:" + id, 1); return RedirectToPage(); }
    public IActionResult OnPostReadAll() { foreach (var n in All) HttpContext.Session.SetInt32("notice:" + n.Id, 1); return RedirectToPage(); }
    public sealed record Notice(int Id, string Title, string Content, string Link);
}
