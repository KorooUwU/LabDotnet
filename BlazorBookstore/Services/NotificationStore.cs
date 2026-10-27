namespace WebApplication1;
public sealed class NotificationStore
{
    private readonly HashSet<int> _read = [];
    public static IReadOnlyList<Notice> All { get; } = [
        new(1, "Chào mừng đến với Mộc Books", "Khám phá sách văn học, thiếu nhi, công nghệ và phát triển bản thân.", "/"),
        new(2, "Khám phá bộ sưu tập", "Tìm theo tên sách hoặc chọn thể loại ở cuối trang.", "/#products"),
        new(3, "Cần hỗ trợ mua hàng?", "Xem hướng dẫn mua hàng và các câu hỏi thường gặp.", "/Support")
    ];
    public event Action? Changed;
    public int UnreadCount => All.Count(x => !_read.Contains(x.Id));
    public bool IsRead(int id) => _read.Contains(id);
    public void MarkRead(int id) { _read.Add(id); Changed?.Invoke(); }
    public void MarkAllRead() { foreach (var notice in All) _read.Add(notice.Id); Changed?.Invoke(); }
    public sealed record Notice(int Id, string Title, string Content, string Link);
}

