# Kiểm thử ba lab

Kiểm thử trình duyệt dùng Playwright + Microsoft Edge. Chạy trên **database kiểm thử** vì script sẽ tạo tài khoản, danh mục và đơn hàng. Không có lệnh xóa database trong script.

Mở ba terminal tại thư mục gốc, đặt cùng connection string kiểm thử trong mỗi terminal trước khi chạy từng project:

```powershell
$env:ConnectionStrings__ShopDatabase='Server=(localdb)\MSSQLLocalDB;Database=MocBooks_LabTests;Trusted_Connection=True;TrustServerCertificate=True'
```

Sau đó chạy lần lượt ba lệnh ở README gốc. Trong terminal thứ tư:

```powershell
cd tests
npm install
npm test
```

Cổng mặc định: Blazor 5119, Razor 5120, MVC 5130. Nếu đổi cổng:

```powershell
$env:LAB_URLS='http://localhost:5240,http://localhost:5220,http://localhost:5230'
npm test
```

Script kiểm tra truy cập công khai, bắt buộc đăng nhập admin, khách bị từ chối quyền quản trị, đăng ký Identity, giỏ hàng, thanh toán Razor/MVC, đơn hàng và tài khoản/danh mục dùng chung, và chặn xem đơn của người khác.

Ngoài script, đã kiểm tra bằng trình duyệt: đặt đơn từ Blazor, CRUD sách/khách hàng/đơn/danh mục, upload ảnh dùng chung, tạo vai trò tùy chỉnh và thu hồi quyền giữa Razor và MVC.
