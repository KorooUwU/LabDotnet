# Mộc Books — ba lab Blazor, Razor Pages và MVC

Ba ứng dụng bán sách dùng **một database SQL Server**, **một file AppDbContext.cs** và chung ASP.NET Core Identity. Mở `BookstoreLabs.sln` bằng Visual Studio hoặc dùng .NET CLI.

| Lab | Project | Cổng HTTP |
| --- | --- | --- |
| Blazor Interactive Server | `BlazorBookstore/WebApplication1.csproj` | http://localhost:5119 |
| Razor Pages | `RazorBookstore/RazorBookstore.csproj` | http://localhost:5120 |
| MVC | `MvcBookstore/MvcBookstore.csproj` | http://localhost:5130 |
| Dữ liệu dùng chung | `Shared/Bookstore.Shared.csproj` | Không chạy riêng |

## Chạy

Cần **.NET SDK 10** và **SQL Server Express LocalDB**. Terminal tại thư mục gốc:

```powershell
SqlLocalDB start MSSQLLocalDB
dotnet restore BookstoreLabs.sln
dotnet build BookstoreLabs.sln
```

Nếu chưa có LocalDB instance, chạy `SqlLocalDB create MSSQLLocalDB -s` thay cho lệnh start.

Mở ba terminal riêng và chạy:

```powershell
# Lab Blazor
dotnet run --project BlazorBookstore/WebApplication1.csproj --launch-profile http
```

```powershell
# Lab Razor Pages
dotnet run --project RazorBookstore/RazorBookstore.csproj --launch-profile http
```

```powershell
# Lab MVC
dotnet run --project MvcBookstore/MvcBookstore.csproj --launch-profile http
```

Bạn có thể chạy một lab hoặc cả ba cùng lúc. Cookie đăng nhập và giỏ hàng tách theo ứng dụng; **tài khoản, vai trò, danh mục, sản phẩm, khách hàng và đơn hàng được lưu chung**.

Tài khoản demo trong Development: **admin / 123456** và **khachhang / 123456**. Mật khẩu của tài khoản cũ đã được người dùng thay đổi sẽ được giữ nguyên khi chuyển sang Identity. Admin đăng nhập rồi vào `/Admin`; khách hàng mới đăng ký tại `/Register`. Đăng ký chỉ cấp vai trò Customer, không cho tự chọn vai trò quản trị.

## Chức năng theo yêu cầu

Cả ba lab đều có:

- Trang bán hàng: danh mục, tìm sách, xem chi tiết, thêm giỏ, đổi số lượng, xóa sách khỏi giỏ, hiển thị tổng số lượng.
- Khách hàng: đăng ký, đăng nhập bằng tên tài khoản/mật khẩu, hiển thị họ tên và đăng xuất.
- Thanh toán: xác nhận địa chỉ nhận hàng, ghi chú, phương thức **COD — thanh toán khi nhận hàng**; lưu đơn và chi tiết đơn, xóa giỏ sau khi thành công và chuyển tới đơn vừa tạo.
- Lịch sử đơn của chính khách hàng tại `/MyOrders`; sửa ID trên URL không xem được đơn của người khác.
- Admin: tổng quan, CRUD/tìm kiếm sản phẩm, khách hàng, danh mục; quản lý đơn hàng và trạng thái; quản lý vai trò, quyền và gán vai trò cho tài khoản.
- Upload bìa JPG/PNG/WebP tối đa 2 MB; ảnh tải lên được cả ba ứng dụng đọc từ `App_Data/product-images` ở thư mục gốc.
- Server validation và antiforgery; không cho xóa danh mục đang có sách hoặc khách hàng có tài khoản/đơn.
- Đơn lưu tên sách và giá tại thời điểm tạo; thay giá hoặc xóa sách không làm thay đổi đơn cũ.

COD là luồng thanh toán thực tế trong bài tập: tạo đơn chờ xác nhận và trả tiền khi nhận hàng. **Chưa tích hợp cổng thanh toán thẻ/chuyển khoản và không tự ghi nhận giao dịch ngân hàng.**

## Một database, một context

File cấu hình duy nhất: [Shared/database.json](Shared/database.json). Database mặc định: **MocMarketMigration**, dùng tiếp database của Blazor.

File context duy nhất đang tham gia build: [Shared/Data/AppDbContext.cs](Shared/Data/AppDbContext.cs), kế thừa `IdentityDbContext<ApplicationUser>`. Cả ba project dùng `ProjectReference` đến Shared, không sao chép context.

```text
BlazorBookstore/Components/      Lab Blazor (.razor)
RazorBookstore/Pages/            Lab Razor Pages (.cshtml + PageModel)
MvcBookstore/Controllers/        Lab MVC (Controller)
MvcBookstore/Views/              Lab MVC (View)
MvcBookstore/Models/             ViewModel cho MVC
Shared/
  Models/                       Entity + input model dùng chung
  Data/AppDbContext.cs          Context duy nhất
  Migrations/                   Bộ migration duy nhất
  database.json                 Connection string dùng chung
  LabSetup.cs                   Cấu hình Identity, policy, migration và dữ liệu mẫu
Legacy/BeforeShared/            Mã trước refactor, chỉ để tham khảo, bị loại khỏi build
App_Data/product-images/        Ảnh upload dùng chung
```

Razor Pages nhận yêu cầu trong `OnGet/OnPost` của PageModel; MVC dùng Controller và ViewModel/View; Blazor dùng component và event handler. Cả ba truy cập EF Core trực tiếp, **không có tầng service nghiệp vụ hoặc repository**. Blazor dùng DbContextFactory tạo context ngắn hạn cho từng thao tác để tránh giữ context suốt circuit. Các lớp giỏ hàng, ảnh và xác thực chỉ xử lý trạng thái giao diện/hạ tầng.

`ApplicationUser : IdentityUser` nằm trong `Shared/Models/ApplicationUser.cs`. Hồ sơ khách hàng nằm ở `Customers`, liên kết qua `ApplicationUser.CustomerId`.

Bảng nghiệp vụ: Products, Categories, Customers, Orders, OrderLines; 7 bảng AspNet* của Identity. Bảng Accounts cũ được giữ để bảo toàn/chuyển tài khoản, không còn dùng đăng nhập. Có thêm bảng lịch sử migration của EF.

Database `MocBooksRazor` của bản Razor cũ không còn được các lab dùng; dữ liệu của database đó vẫn được giữ. Không tự gộp dữ liệu riêng từ database cũ này vào database chính.

## Migration Code First

Ứng dụng tự chạy `Database.MigrateAsync()` và khởi tạo dữ liệu khi khởi động. Hoặc chạy từ thư mục gốc:

```powershell
dotnet ef migrations list --project Shared/Bookstore.Shared.csproj --startup-project Shared/Bookstore.Shared.csproj --context AppDbContext
dotnet ef database update --project Shared/Bookstore.Shared.csproj --startup-project Shared/Bookstore.Shared.csproj --context AppDbContext
```

Sau khi tự sửa entity/context:

```powershell
dotnet ef migrations add TenThayDoi --project Shared/Bookstore.Shared.csproj --startup-project Shared/Bookstore.Shared.csproj --context AppDbContext --output-dir Migrations --namespace WebApplication1.Migrations
dotnet ef database update --project Shared/Bookstore.Shared.csproj --startup-project Shared/Bookstore.Shared.csproj --context AppDbContext
```

Không tạo InitialCreate lại ở từng lab. Migration `SharedIdentityCategoriesCheckout` bổ sung cấu trúc Identity/danh mục/thanh toán, giữ dữ liệu sẵn có. Khi khởi động lần đầu, tài khoản bảng Accounts được chuyển sang Identity với hash mật khẩu cũ; các sách cũ được gắn vào danh mục tương ứng.

Database hiện tại đã được sao lưu trước nâng cấp tại `App_Data/Backups/MocMarketMigration-before-shared-20261026175902.bak`.

Ba cách ánh xạ EF vẫn được minh họa: convention cho Product.Id, Data Annotations trong Customer/Category và Fluent API trong AppDbContext/OrderConfiguration.

## Phân quyền linh động

Đăng nhập admin, mở `/Admin/Roles`:

1. Tạo vai trò, ví dụ `NhanVienDanhMuc`.
2. Chọn `Admin.Access` và `Categories.Manage`, bấm Lưu quyền.
3. Gán vai trò đó cho một tài khoản, bấm Lưu vai trò.
4. Tài khoản đó đăng nhập lại: vào được quản lý danh mục, không vào được quản lý sách/khách hàng nếu chưa được cấp quyền.

Các quyền: Admin.Access, Products.Manage, Customers.Manage, Categories.Manage, Orders.Manage, Roles.Manage. Lưu trong IdentityRoleClaims, gán vai trò bằng IdentityUserRoles. Server kiểm tra database khi xét quyền, không chỉ ẩn menu. Thay quyền cập nhật security stamp; phiên bị ảnh hưởng phải đăng nhập lại, circuit Blazor kiểm tra lại tối đa mỗi 30 giây và kiểm tra quyền trước các thao tác ghi.

Vai trò Admin hệ thống giữ đủ quyền; không cho bỏ vai trò Admin của quản trị viên cuối cùng.

## Kiểm tra

`dotnet build BookstoreLabs.sln` kiểm tra cả ba lab và Shared. Hướng dẫn kiểm thử trình duyệt tại [tests/README.md](tests/README.md).

Đã kiểm tra trên database riêng: đăng ký Identity, đăng nhập liên lab, COD ở cả ba lab, lịch sử đơn dùng chung, quyền truy cập admin, quyền vai trò tùy chỉnh/thu hồi quyền, bảo vệ đơn khách hàng, CRUD và ảnh upload dùng chung.

Giỏ Blazor lưu theo tab trình duyệt; Razor/MVC dùng Session trong bộ nhớ server (hết hạn sau 8 giờ không hoạt động, mất khi server khởi động lại). Dữ liệu đơn đã xác nhận được lưu bền vững trong SQL Server.


#   L a b D o t n e t  
 