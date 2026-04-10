# HƯỚNG DẪN SỬ DỤNG HỆ THỐNG BẢO MẬT JWT & IDENTITY

## Tổng quan

Dự án này minh họa việc implement hệ thống bảo mật và phân quyền sử dụng:

- **ASP.NET Core Identity** để quản lý người dùng
- **JSON Web Token (JWT)** để xác thực
- **2 Role**: Admin và User

---

## Cấu trúc Dự án

```
Chapter06/
├── MvApplication/              # Application Layer
│   ├── Models/                 # Domain models
│   │   ├── User.cs            # User model với enum UserRole (Admin, User)
│   │   └── TokenModel.cs      # JWT token models
│   ├── Ports/Security/        # Interfaces
│   │   ├── IJwtService.cs
│   │   ├── IAuthService.cs
│   │   ├── ICurrentUser.cs
│   │   └── IUserService.cs
│   └── UseCases/
│       ├── Auth/              # Auth use cases
│       │   ├── Register/
│       │   ├── Login/
│       │   ├── Refresh/
│       │   ├── Logout/
│       │   └── GetProfile/
│       └── User/              # User management
│           └── GetAllUsers/
│
├── MvInfrastructure/          # Infrastructure Layer
│   ├── Identity/
│   │   └── AppUser.cs         # IdentityUser<Guid> entity
│   ├── Persistence/
│   │   └── AppDbContext.cs    # DbContext với Identity
│   ├── Options/
│   │   └── JwtOptions.cs      # JWT configuration
│   ├── Adapters/Security/
│   │   ├── AuthService.cs     # Authentication logic
│   │   └── UserService.cs     # User queries
│   └── Extensions/
│       └── IdentityExtensions.cs
│
└── MvPresentation/            # Presentation Layer
    ├── Controllers/
    │   ├── Admin/             # Admin endpoints
    │   │   ├── AuthController.cs
    │   │   └── UserManagementController.cs
    │   └── User/              # User endpoints
    │       └── AuthController.cs
    ├── Adapters/Security/
    │   ├── JwtService.cs      # JWT generation
    │   └── CurrentUser.cs     # Current user resolver
    └── Extensions/
        └── PresentationExtensions.cs  # JWT authentication config
```

---

## Cài đặt và Chạy

### 1. Cài đặt Dependencies

```bash
cd Chapter06/MvPresentation
dotnet restore
```

### 2. Cấu hình Database

Cập nhật connection string trong `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Chapter06IdentityDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Tạo Database và Migration

```bash
# Từ thư mục MvPresentation
dotnet ef migrations add InitialCreate -p ../MvInfrastructure -s .
dotnet ef database update -p ../MvInfrastructure -s .
```

### 4. Chạy ứng dụng

```bash
dotnet run
```

Truy cập Swagger UI:

- **User API**: https://localhost:5001/swagger (document: user)
- **Admin API**: https://localhost:5001/swagger (document: admin)

---

## API Endpoints

### User Endpoints (`/api/user/auth`)

#### 1. Đăng ký User mới

```http
POST /api/user/auth/register
Content-Type: application/json

{
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "Strong@Password123"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "access": {
      "token": "eyJhbGciOiJIUzI1NiIs...",
      "expiredAt": "2026-02-14T10:30:00Z"
    },
    "refresh": {
      "token": "eyJhbGciOiJIUzI1NiIs...",
      "expiredAt": "2026-02-15T09:30:00Z"
    }
  }
}
```

#### 2. Đăng nhập

```http
POST /api/user/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Strong@Password123"
}
```

#### 3. Refresh Token

```http
POST /api/user/auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

#### 4. Logout

```http
POST /api/user/auth/logout
Authorization: Bearer {access-token}
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

#### 5. Lấy Profile hiện tại (User Role required)

```http
GET /api/user/auth/profile
Authorization: Bearer {access-token}
```

**Response:**

```json
{
  "success": true,
  "message": "Lấy thông tin thành công",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "userName": "john_doe",
    "email": "john@example.com",
    "role": "User",
    "createdAt": "2026-02-14T09:00:00Z"
  }
}
```

---

### Admin Endpoints (`/api/admin`)

#### 1. Đăng ký Admin

```http
POST /api/admin/auth/register
Content-Type: application/json

{
  "userName": "admin_user",
  "email": "admin@example.com",
  "password": "Admin@Strong123"
}
```

#### 2. Đăng nhập Admin

```http
POST /api/admin/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin@Strong123"
}
```

#### 3. Lấy Profile Admin (Admin Role required)

```http
GET /api/admin/auth/profile
Authorization: Bearer {access-token}
```

#### 4. Lấy danh sách tất cả Users (Admin Only)

```http
GET /api/admin/users
Authorization: Bearer {admin-access-token}
```

**Response:**

```json
{
  "success": true,
  "message": "Lấy danh sách người dùng thành công",
  "data": [
    {
      "id": "...",
      "userName": "john_doe",
      "email": "john@example.com",
      "role": "User",
      "createdAt": "2026-02-14T09:00:00Z"
    },
    {
      "id": "...",
      "userName": "admin_user",
      "email": "admin@example.com",
      "role": "Admin",
      "createdAt": "2026-02-14T08:00:00Z"
    }
  ]
}
```

---

## Authentication Flow

### 1. Đăng ký và Đăng nhập

```
┌─────────┐                    ┌────────────┐                   ┌──────────┐
│ Client  │                    │   Server   │                   │ Database │
└────┬────┘                    └─────┬──────┘                   └────┬─────┘
     │                               │                                │
     │  POST /auth/register          │                                │
     │ {email, password, userName}   │                                │
     ├──────────────────────────────>│                                │
     │                               │                                │
     │                               │  Create User with hashed pwd   │
     │                               ├───────────────────────────────>│
     │                               │                                │
     │                               │  User Created                  │
     │                               │<───────────────────────────────┤
     │                               │                                │
     │                               │  Generate JWT Tokens           │
     │                               │  (Access + Refresh)            │
     │                               │                                │
     │  {access, refresh} tokens     │                                │
     │<──────────────────────────────┤                                │
     │                               │                                │
```

### 2. Authorized Request

```
┌─────────┐                    ┌────────────┐
│ Client  │                    │   Server   │
└────┬────┘                    └─────┬──────┘
     │                               │
     │  GET /user/auth/profile       │
     │  Authorization: Bearer {JWT}  │
     ├──────────────────────────────>│
     │                               │
     │                               │  1. Validate JWT signature
     │                               │  2. Check expiration
     │                               │  3. Extract claims
     │                               │  4. Check role (User)
     │                               │
     │  User Profile Data            │
     │<──────────────────────────────┤
     │                               │
```

### 3. Token Refresh Flow

```
┌─────────┐                    ┌────────────┐                   ┌──────────┐
│ Client  │                    │   Server   │                   │ Database │
└────┬────┘                    └─────┬──────┘                   └────┬─────┘
     │                               │                                │
     │  Access Token expired         │                                │
     │                               │                                │
     │  POST /auth/refresh           │                                │
     │  {refreshToken}               │                                │
     ├──────────────────────────────>│                                │
     │                               │                                │
     │                               │  Validate refresh token        │
     │                               │  Check SecurityStamp           │
     │                               ├───────────────────────────────>│
     │                               │                                │
     │                               │  User data                     │
     │                               │<───────────────────────────────┤
     │                               │                                │
     │                               │  Generate new tokens           │
     │                               │                                │
     │  New {access, refresh} tokens │                                │
     │<──────────────────────────────┤                                │
     │                               │                                │
```

---

## Cấu hình JWT

File `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJwtThatIsAtLeast32CharactersLong!",
    "Issuer": "MvPresentation",
    "Audience": "MvPresentation",
    "AccessTokenExpiryMinutes": 30,
    "RefreshTokenExpiryMinutes": 1440
  }
}
```

### Best Practices:

1. **Secret Key**:
   - Tối thiểu 32 ký tự
   - Lưu trong Environment Variables trong Production
   - Không commit vào Git

2. **Token Expiry**:
   - **Access Token**: Ngắn (15-60 phút) - Giảm rủi ro nếu bị đánh cắp
   - **Refresh Token**: Dài hơn (1-7 ngày) - Tránh user phải login liên tục

3. **Security Stamp**:
   - Dùng để invalidate tất cả tokens khi:
     - Đổi password
     - Phát hiện security breach
     - Admin force logout

---

## Password Policy

Mặc định trong `IdentityExtensions.cs`:

```csharp
options.Password.RequireDigit = true;            // Bắt buộc có số
options.Password.RequireLowercase = true;        // Bắt buộc chữ thường
options.Password.RequireUppercase = true;        // Bắt buộc chữ hoa
options.Password.RequireNonAlphanumeric = true;  // Bắt buộc ký tự đặc biệt
options.Password.RequiredLength = 8;             // Tối thiểu 8 ký tự
```

Ví dụ password hợp lệ: `Strong@Pass123`

---

## Account Lockout

Tự động khóa tài khoản sau 5 lần đăng nhập sai:

```csharp
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.AllowedForNewUsers = true;
```

---

## Authorization Examples

### 1. Role-based Authorization

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase {
  // Chỉ Admin mới truy cập được
}

[Authorize(Roles = "User")]
public async Task<IActionResult> UserAction() {
  // Chỉ User mới truy cập được
}

[Authorize(Roles = "Admin,User")]
public async Task<IActionResult> BothRoles() {
  // Admin hoặc User đều truy cập được
}
```

### 2. Policy-based Authorization

Trong `PresentationExtensions.cs`:

```csharp
services.AddAuthorization(options => {
  options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
  options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});
```

Sử dụng:

```csharp
[Authorize(Policy = "AdminOnly")]
public class SecureController : ControllerBase {
  // Implementation
}
```

### 3. Lấy Current User trong Use Case

```csharp
public class MyHandler(ICurrentUser currentUser) : IRequestHandler<MyQuery, Result> {
  public async Task<Result> Handle(MyQuery request, CancellationToken ct) {
    var userId = currentUser.User.Id;
    var userName = currentUser.User.UserName;
    var role = currentUser.User.Role;

    // Business logic with current user
  }
}
```

---

## Testing với Postman/curl

### 1. Đăng ký User

```bash
curl -X POST https://localhost:5001/api/user/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "Test@Password123"
  }'
```

### 2. Lưu tokens và sử dụng cho requests tiếp theo

```bash
# Lưu access token vào biến
ACCESS_TOKEN="eyJhbGciOiJIUzI1NiIs..."

# Gọi protected endpoint
curl -X GET https://localhost:5001/api/user/auth/profile \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

### 3. Test Admin endpoint (sẽ fail nếu dùng User token)

```bash
curl -X GET https://localhost:5001/api/admin/users \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

Kết quả: `403 Forbidden` vì User role không có quyền truy cập.

---

## Troubleshooting

### 1. Token không hợp lệ

- **Kiểm tra**: Token đã hết hạn chưa?
- **Giải pháp**: Sử dụng refresh token để lấy token mới

### 2. 403 Forbidden

- **Kiểm tra**: Role của user có phù hợp với endpoint không?
- **Giải pháp**: Đăng nhập với user có role phù hợp

### 3. Database connection error

- **Kiểm tra**: Connection string trong appsettings
- **Giải pháp**: Chạy lại migration: `dotnet ef database update`

### 4. SecurityStamp mismatch

- **Nguyên nhân**: User đã đổi password hoặc bị force logout
- **Giải pháp**: Đăng nhập lại để lấy token mới

---

## Các tính năng nâng cao có thể mở rộng

1. **Two-Factor Authentication (2FA)**
2. **Email Confirmation**
3. **Password Reset**
4. **Token Blacklisting với Redis**
5. **Claim-based Authorization**
6. **OAuth/External Login (Google, Facebook)**
7. **Rate Limiting**
8. **Audit Logging**

---

## Tài liệu tham khảo

- [IDENTITY_JWT_GUIDE.md](./IDENTITY_JWT_GUIDE.md) - Hướng dẫn chi tiết về từng component
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT.io](https://jwt.io/) - Debug JWT tokens
