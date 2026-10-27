# Lab Blazor — Mộc Books

Project Blazor nằm riêng tại đây, dùng chung database, Identity và AppDbContext với Razor Pages và MVC.

Từ thư mục gốc chứa BookstoreLabs.sln:

```powershell
dotnet run --project BlazorBookstore/WebApplication1.csproj --launch-profile http
```

Hoặc mở terminal ngay trong BlazorBookstore và chạy `dotnet run --launch-profile http`.

Mở http://localhost:5119. Tên file project vẫn là WebApplication1.csproj để giữ nguyên assembly và namespace hiện có.

Cấu hình database ở `../Shared/database.json`; ảnh tải lên vẫn dùng `../App_Data/product-images`. Khi nộp bài, giữ BlazorBookstore, RazorBookstore, MvcBookstore và Shared trong cùng thư mục cha. Xem [hướng dẫn chung](../README.md).
