namespace MvcBookstore.Models;
public class NotificationsModel
{
    public static IReadOnlyList<Notice> All { get; } = [
        new(1,"Chào mừng đến với Mộc Books","Khám phá sách văn học, thiếu nhi, công nghệ và phát triển bản thân.","/"),
        new(2,"Khám phá bộ sưu tập","Tìm sách theo tên, tác giả hoặc thể loại.","/#products"),
        new(3,"Cần hỗ trợ mua hàng?","Xem hướng dẫn mua hàng và câu hỏi thường gặp.","/Support")
    ];
    public HashSet<int> Read { get; set; } = [];
    public bool IsRead(int id) => Read.Contains(id);
    public int UnreadCount => All.Count(x => !IsRead(x.Id));
    public sealed record Notice(int Id,string Title,string Content,string Link);
}
