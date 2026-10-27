namespace WebApplication1;

public static class SeedBooks
{

    // Fictional sample titles; replace with your actual book catalog in admin.
    public static List<Product> Items { get; } = [
        new(1, "Những ngày đọc chậm", "Sách mẫu · Những câu chuyện về một nhịp sống bình yên.", "VĂN HỌC", 125000, "", "clay", "/images/books/doc-cham.svg", "An Mộc (mẫu)", "Nhà xuất bản mẫu", 2026, "", 208),
        new(2, "Một khu vườn trong tâm trí", "Sách mẫu · Dành một khoảng lặng để hiểu bản thân.", "PHÁT TRIỂN BẢN THÂN", 149000, "", "sage", "/images/books/khu-vuon.svg", "Minh An (mẫu)", "Nhà xuất bản mẫu", 2026, "", 256),
        new(3, "Chuyến đi của hạt mầm", "Sách mẫu · Câu chuyện nhỏ dành cho những trí tưởng tượng lớn.", "THIẾU NHI", 89000, "", "amber", "/images/books/hat-mam.svg", "Mai Lá (mẫu)", "Nhà xuất bản mẫu", 2026, "", 48),
        new(4, "Bắt đầu với C#", "Sách mẫu · Làm quen lập trình qua những ví dụ gần gũi.", "CÔNG NGHỆ", 195000, "", "terracotta", "/images/books/csharp.svg", "Quang Nam (mẫu)", "Nhà xuất bản mẫu", 2026, "", 320)
    ];
}

