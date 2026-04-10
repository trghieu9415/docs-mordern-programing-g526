# CHƯƠNG 6: BẢO MẬT VÀ PHÂN QUYỀN DỰA TRÊN JWT VÀ IDENTITY

Dự án này chứa mã nguồn mẫu về thiết lập hệ thống bảo mật, xác thực người dùng và phân quyền truy cập trong ứng dụng ASP.NET Core sử dụng JWT và thư viện Identity.

## 🚀 Quick Start

```bash
# 1. Di chuyển đến thư mục Presentation
cd Chapter06/MvPresentation

# 2. Restore dependencies
dotnet restore

# 3. Tạo database
dotnet ef migrations add InitialCreate -p ../MvInfrastructure -s . -o Persistence/Migrations
dotnet ef database update -p ../MvInfrastructure -s .

# 4. Chạy ứng dụng
dotnet run

# 5. Truy cập Swagger UI
# https://localhost:5001/swagger
```

**Default Admin Account:**

- Email: `admin@example.com`
- Password: `Admin@123`

## 📚 Tài liệu

- **[USAGE_GUIDE.md](./USAGE_GUIDE.md)** - Hướng dẫn sử dụng API và testing
- **[IDENTITY_JWT_GUIDE.md](./IDENTITY_JWT_GUIDE.md)** - Tài liệu chi tiết về implementation
- **[MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md)** - Hướng dẫn setup database
- **[test.http](./test.http)** - HTTP requests để test API

## 🎯 Features

✅ **ASP.NET Core Identity** - Quản lý user và authentication
✅ **JWT Authentication** - Access token & Refresh token
✅ **Role-based Authorization** - 2 roles: Admin và User
✅ **Password Policy** - Yêu cầu password mạnh
✅ **Account Lockout** - Tự động khóa sau 5 lần login sai
✅ **Secure Password Hashing** - Sử dụng Identity's password hasher
✅ **Clean Architecture** - Tách biệt layers rõ ràng
✅ **CQRS với MediatR** - Request/Response pattern
✅ **FluentValidation** - Validation cho inputs

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────┐
│                    MvPresentation                        │
│  - Controllers (Admin, User)                            │
│  - JWT Service (Token Generation)                       │
│  - Current User Service                                 │
│  - Authentication Middleware Configuration              │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                   MvApplication                          │
│  - Use Cases (CQRS Handlers)                            │
│  - Models (User, TokenModel)                            │
│  - Ports/Interfaces (IAuthService, IJwtService)         │
│  - Validation (FluentValidation)                        │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                 MvInfrastructure                         │
│  - Identity (AppUser entity)                            │
│  - DbContext (with Identity tables)                     │
│  - Auth Service (Login, Register, Refresh)              │
│  - User Service (User queries)                          │
└────────────────────┬────────────────────────────────────┘
                     │
                ┌────▼─────┐
                │ Database │
                │  (MSSQL) │
                └──────────┘
```

## 📁 Cấu trúc Project

### MvApplication (Application Layer)

- `Models/` - Domain models (User, TokenModel, UserRole enum)
- `Ports/Security/` - Interfaces cho security services
- `UseCases/Auth/` - CQRS handlers cho authentication
  - Register, Login, Refresh, Logout, GetProfile
- `UseCases/User/` - User management use cases
- `DTOs/` - Data transfer objects
- `Behaviors/` - MediatR pipeline behaviors (validation)

### MvInfrastructure (Infrastructure Layer)

- `Identity/AppUser.cs` - Identity user entity (kế thừa IdentityUser<Guid>)
- `Persistence/AppDbContext.cs` - DbContext với Identity integration
- `Adapters/Security/` - Implementation của auth services
- `Options/JwtOptions.cs` - JWT configuration model
- `Extensions/IdentityExtensions.cs` - DI setup cho Identity

### MvPresentation (Presentation Layer)

- `Controllers/User/` - User endpoints (/api/user/\*)
- `Controllers/Admin/` - Admin endpoints (/api/admin/\*)
- `Adapters/Security/` - JWT service & Current user resolver
- `Extensions/PresentationExtensions.cs` - JWT authentication middleware

## 🔑 Các thành phần chính

### 1. Hệ thống danh tính (Identity System)

- Cấu hình ASP.NET Core Identity để quản lý người dùng
- Custom AppUser entity với properties bổ sung (Role, CreatedAt, LastLoginAt)
- Password hashing tự động
- Account lockout sau nhiều lần đăng nhập thất bại
- Security stamp cho token invalidation

### 2. Xác thực với JSON Web Token (JWT)

- **Access Token**: 30 phút (ngắn hạn, dùng cho API calls)
- **Refresh Token**: 24 giờ (dài hạn, dùng để lấy access token mới)
- Claims-based: UserId, Email, Role, SecurityStamp
- HMAC SHA256 signing algorithm
- Token validation trong middleware

### 3. Chiến lược phân quyền (Authorization)

- **Role-based Authorization**: 2 roles (Admin, User)
- Controllers riêng cho từng role
- `[Authorize(Roles = "Admin")]` attribute
- Policy-based authorization có thể mở rộng

### 4. Quản lý phiên đăng nhập và Refresh Token

- Refresh token flow để gia hạn access token
- Logout mechanism
- SecurityStamp validation để invalidate tokens sau khi đổi password
- Có thể mở rộng với token blacklisting (Redis)

### 5. Cấu hình bảo mật và Identity Options

- **Password Policy**:
  - Tối thiểu 8 ký tự
  - Yêu cầu chữ hoa, chữ thường, số, ký tự đặc biệt
- **Lockout Settings**:
  - Khóa 15 phút sau 5 lần đăng nhập sai
- **Unique Email**: Mỗi email chỉ đăng ký 1 tài khoản

## 🔒 Security Best Practices

✅ Password không lưu dưới dạng plain text (hashed với Identity)
✅ JWT Secret key >= 32 characters
✅ Access token ngắn hạn (giảm rủi ro nếu bị đánh cắp)
✅ HTTPS required
✅ SecurityStamp để invalidate tokens
✅ Account lockout chống brute force
✅ Validation cho tất cả inputs
✅ Unique email constraint

## 🧪 Testing

### Sử dụng Swagger UI

1. Mở https://localhost:5001/swagger
2. Chọn document "user" hoặc "admin"
3. Thử các endpoints

### Sử dụng VS Code REST Client

1. Cài extension "REST Client"
2. Mở file `test.http`
3. Click "Send Request" trên mỗi request

### Flow test cơ bản

1. Đăng ký user mới (`POST /api/user/auth/register`)
2. Copy access token từ response
3. Click "Authorize" button ở Swagger
4. Paste token và test protected endpoints
5. Test Admin endpoints (sẽ fail với 403)
6. Login với admin account
7. Test Admin endpoints với admin token (success)

## 📊 API Endpoints Summary

| Endpoint                   | Method | Role   | Description       |
| -------------------------- | ------ | ------ | ----------------- |
| `/api/user/auth/register`  | POST   | Public | Đăng ký user      |
| `/api/user/auth/login`     | POST   | Public | Đăng nhập         |
| `/api/user/auth/refresh`   | POST   | Public | Refresh token     |
| `/api/user/auth/logout`    | POST   | User   | Đăng xuất         |
| `/api/user/auth/profile`   | GET    | User   | Xem profile       |
| `/api/admin/auth/register` | POST   | Public | Đăng ký admin     |
| `/api/admin/auth/login`    | POST   | Public | Đăng nhập admin   |
| `/api/admin/auth/profile`  | GET    | Admin  | Xem profile admin |
| `/api/admin/users`         | GET    | Admin  | Xem tất cả users  |

## 🛠️ Technologies

- **ASP.NET Core 8.0**
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **ASP.NET Core Identity** - User management
- **JWT Bearer Authentication** - Token-based auth
- **MediatR** - CQRS pattern
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **Serilog** - Logging
- **NSwag** - Swagger/OpenAPI

## 🔄 Extending

Code có thể mở rộng thêm:

- [ ] Email confirmation
- [ ] Password reset via email
- [ ] Two-factor authentication (2FA)
- [ ] External login providers (Google, Facebook)
- [ ] Token blacklisting với Redis
- [ ] Claim-based authorization
- [ ] Rate limiting
- [ ] Audit logging
- [ ] User profile management
- [ ] Role management UI

## 📝 Notes

- Đây là code ví dụ cho mục đích học tập
- Trong production cần thêm logging, monitoring
- Secret keys nên lưu trong Environment Variables hoặc Azure Key Vault
- Nên enable HTTPS và HSTS
- Consider thêm rate limiting để chống abuse
- Database backup định kỳ

## 🙋 FAQs

**Q: Tại sao cần cả Access Token và Refresh Token?**
A: Access token ngắn (30 phút) giảm rủi ro nếu bị đánh cắp. Refresh token dài (24h) giúp user không phải login liên tục.

**Q: Làm sao để invalidate token khi user đổi password?**
A: Update SecurityStamp trong database. Token cũ sẽ không valid nữa vì SecurityStamp không match.

**Q: User có thể có nhiều role không?**
A: Implementation hiện tại là 1 role per user. Có thể mở rộng với bảng UserRoles nếu cần.

**Q: JWT có được lưu ở server không?**
A: Không, JWT là stateless. Server chỉ validate signature. Muốn revoke phải dùng blacklist (Redis).

## 📞 Support

Xem thêm chi tiết trong các file tài liệu:

- Hướng dẫn API: [USAGE_GUIDE.md](./USAGE_GUIDE.md)
- Chi tiết implementation: [IDENTITY_JWT_GUIDE.md](./IDENTITY_JWT_GUIDE.md)
- Database setup: [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md)
