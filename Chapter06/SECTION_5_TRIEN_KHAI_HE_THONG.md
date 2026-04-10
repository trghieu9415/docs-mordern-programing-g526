# 5. Triển khai hệ thống

## a. Cấu hình mô hình dữ liệu
Hệ thống sử dụng ASP.NET Core Identity để quản lý thông tin người dùng. Thực thể chính là `ApplicationUser`, kế thừa từ `IdentityUser<Guid>`, đồng thời bổ sung thêm các thuộc tính `Role`, `CreatedAt` và `LastLoginAt`.

Thuộc tính `Role` được ánh xạ từ `enum UserRole` gồm hai giá trị `Admin` và `User`. Để phù hợp với yêu cầu bài toán, hệ thống chỉ quản lý người dùng mà không sử dụng bảng `Role` hay `RoleClaim` của Identity.

Trong `DbContext`, hệ thống sử dụng `IdentityUserContext<ApplicationUser, Guid>` thay vì `IdentityDbContext`, nhờ đó cơ sở dữ liệu chỉ phát sinh các bảng cần thiết cho người dùng và xác thực. Ngoài ra, trường `Email` được cấu hình unique index để đảm bảo tính duy nhất.

Gợi ý ảnh code:
- `ApplicationUser.cs`
- `AppDbContext.cs`

## b. Cấu hình Migration và Seeding dữ liệu
Hệ thống sử dụng Entity Framework Core Migration để quản lý cấu trúc cơ sở dữ liệu PostgreSQL. PostgreSQL được triển khai bằng Docker nhằm đảm bảo môi trường chạy ổn định và dễ kiểm thử.

Khi ứng dụng khởi động, hệ thống tự động thực hiện migrate database nếu schema chưa tồn tại hoặc chưa đồng bộ. Phần dữ liệu mẫu được tách riêng thành một seeder độc lập thay vì gắn cứng vào migration.

Seeder hiện tạo sẵn hai tài khoản phục vụ kiểm thử:
- `admin@example.com / Admin@123`
- `user@example.com / User@123`

Lệnh seed dữ liệu:

```powershell
dotnet run --project MvPresentation -- --seed-data
```

Gợi ý ảnh code:
- `docker-compose.yml`
- `IdentityDataSeeder.cs`

## c. Triển khai xác thực và phân quyền
Phần xác thực được xây dựng xoay quanh `IAuthService`, bao gồm các chức năng chính: đăng nhập, làm mới token, đăng xuất và thu hồi token.

Khi người dùng đăng nhập thành công, hệ thống cấp phát đồng thời:
- `Access Token` có thời hạn 30 phút
- `Refresh Token` có thời hạn 24 giờ

Trong Access Token, hệ thống nhúng các claims bắt buộc:
- `NameIdentifier`
- `Role`
- `SecurityStamp`

JWT được cấu hình với `ClockSkew = TimeSpan.Zero`, đảm bảo không có thời gian dung sai khi kiểm tra hạn sử dụng token. Việc phân quyền được thực hiện dựa trên giá trị `Role` trong claims.

Gợi ý ảnh code:
- `IdentityExtensions.cs`
- `PresentationExtensions.cs`
- `JwtService.cs`

## d. Nghiệp vụ và tối ưu truy vấn
Ở nghiệp vụ đăng nhập, hệ thống sử dụng trực tiếp `UserManager` của Identity để kiểm tra mật khẩu, ghi nhận số lần đăng nhập sai và kích hoạt cơ chế khóa tài khoản. Nếu đăng nhập sai quá 5 lần, tài khoản sẽ bị khóa trong 15 phút.

Ở nghiệp vụ refresh token, hệ thống thực hiện:
1. Parse refresh token để lấy `UserId`
2. Lấy `SecurityStamp` từ token
3. Truy vấn người dùng hiện tại trong database
4. So sánh `SecurityStamp` trong token với `SecurityStamp` trong database
5. Nếu trùng khớp thì cấp token mới, nếu không thì từ chối

Cách xử lý này giúp vô hiệu hóa toàn bộ token cũ khi người dùng thay đổi thông tin bảo mật quan trọng hoặc khi cần thu hồi phiên đăng nhập.

Về tối ưu truy vấn, hệ thống sử dụng các API sẵn có của Identity như `FindByEmailAsync`, `FindByIdAsync`, `CheckPasswordAsync`, giúp giảm thao tác truy vấn thủ công. Việc sử dụng unique index trên `Email` cũng giúp tra cứu người dùng nhanh và chính xác hơn.

Gợi ý ảnh code:
- `AuthService.cs`

## Ảnh minh chứng nên chèn
- Hình 5.1. Kết quả đăng nhập thành công trả về Access Token và Refresh Token.
- Hình 5.2. Kết quả decode Access Token trên jwt.io hiển thị `UserId`, `Role`, `SecurityStamp`.
- Hình 5.3. Kết quả đăng nhập sai quá 5 lần, tài khoản bị khóa 15 phút.
- Hình 5.4. Kết quả cập nhật `SecurityStamp` trong DBeaver.
- Hình 5.5. Kết quả gọi API Refresh Token sau khi thay đổi `SecurityStamp` bị từ chối.
