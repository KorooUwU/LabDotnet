# Lab ASP.NET Core MVC — Mộc Books

Ứng dụng MVC độc lập dùng chung database/context/Identity với Blazor và Razor Pages. Terminal tại thư mục gốc:

```powershell
dotnet run --project MvcBookstore/MvcBookstore.csproj --launch-profile http
```

Mở http://localhost:5130. Admin demo: `admin / 123456`; khách demo: `khachhang / 123456`. Khách mới đăng ký tại /Register.

- `Controllers`: nhận request, validation, truy cập AppDbContext/Identity trực tiếp và trả View hoặc redirect.
- `Models`: ViewModel/Input cho giao diện, không chứa truy vấn database.
- `Views`: View Razor .cshtml, form tag helper và layout.
- Không có PageModel/Razor Pages hoặc component Blazor trong lab này; không có service nghiệp vụ/repository.

HomeController hiển thị sản phẩm; CartController xử lý giỏ; AccountController đăng ký/đăng nhập; CheckoutController xác nhận COD và lịch sử riêng; AdminController quản lý dữ liệu và quyền.

Connection string chỉ sửa ở `../Shared/database.json`; AppDbContext và migration dùng chung ở Shared. Xem [hướng dẫn chung](../README.md) về migration, quyền và kiểm thử.
