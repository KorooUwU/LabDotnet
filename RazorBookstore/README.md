# Lab Razor Pages — Mộc Books

Đây là ứng dụng Razor Pages độc lập, dùng chung database/context/Identity với Blazor và MVC. Mở terminal tại thư mục gốc:

```powershell
dotnet run --project RazorBookstore/RazorBookstore.csproj --launch-profile http
```

Mở http://localhost:5120. Admin demo: `admin / 123456`; khách demo: `khachhang / 123456`. Khách mới đăng ký tại /Register. Các trang admin đã yêu cầu đăng nhập và quyền tương ứng.

- `Pages/*.cshtml`: View HTML + cú pháp Razor.
- `Pages/*.cshtml.cs`: PageModel xử lý GET/POST, trực tiếp dùng AppDbContext và Identity.
- `Pages/Admin`: tổng quan, CRUD sách/khách/danh mục/đơn và vai trò.
- `Program.cs`: đăng ký Razor Pages, Session và quy tắc phân quyền theo trang.
- Không có service nghiệp vụ hoặc repository.

Trang /Checkout xác nhận đơn COD, sau đó chuyển /MyOrders?id=...; lịch sử được lọc theo tài khoản đang đăng nhập. Giỏ hàng dùng Session, còn đơn lưu SQL Server.

Connection string chỉ sửa ở `../Shared/database.json`; AppDbContext và migration nằm trong Shared. Database cũ MocBooksRazor vẫn còn nhưng không còn được lab dùng. Xem [hướng dẫn chung](../README.md) để chạy migration, phân quyền và kiểm thử.
