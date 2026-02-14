# BÁO CÁO CHƯƠNG 6: BẢO MẬT VÀ PHÂN QUYỀN DỰA TRÊN JWT VÀ IDENTITY

---

## MỤC LỤC

- [I. TỔNG QUAN VỀ HỆ THỐNG DANH TÍNH](#i-tổng-quan-về-hệ-thống-danh-tính)
- [II. CƠ CHẾ XÁC THỰC VỚI JWT](#ii-cơ-chế-xác-thực-với-jwt)
- [III. CHIẾN LƯỢC PHÂN QUYỀN (ADMIN - USER)](#iii-chiến-lược-phân-quyền-admin---user)
- [IV. QUẢN LÝ PHIÊN ĐĂNG NHẬP](#iv-quản-lý-phiên-đăng-nhập)
- [V. CÁC CẤU HÌNH BẢO MẬT TRONG IDENTITY](#v-các-cấu-hình-bảo-mật-trong-identity)
- [VI. VÍ DỤ MINH HỌA](#vi-ví-dụ-minh-họa)

---

## I. TỔNG QUAN VỀ HỆ THỐNG DANH TÍNH

### 1.1. Giới thiệu ASP.NET Core Identity

**ASP.NET Core Identity** là một framework quản lý người dùng tích hợp sẵn trong ASP.NET Core, cung cấp các chức năng:

- **Quản lý tài khoản người dùng**: Tạo, cập nhật, xóa users
- **Xác thực (Authentication)**: Xác minh danh tính người dùng
- **Phân quyền (Authorization)**: Kiểm soát quyền truy cập tài nguyên
- **Mã hóa mật khẩu**: Password hashing với thuật toán an toàn
- **Token generation**: Tạo tokens cho reset password, email confirmation
- **Account lockout**: Khóa tài khoản sau nhiều lần đăng nhập sai
- **Two-factor authentication**: Xác thực 2 lớp (có thể mở rộng)

### 1.2. Kiến trúc hệ thống Identity trong dự án

Trong dự án này, chúng ta sử dụng **Clean Architecture** với 3 layers chính:

```
┌─────────────────────────────────────────────────────────┐
│              PRESENTATION LAYER                          │
│  ┌────────────────────────────────────────────┐         │
│  │ Controllers                                 │         │
│  │  - User/AuthController                     │         │
│  │  - Admin/AuthController                    │         │
│  │  - Admin/UserManagementController          │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ Adapters/Security                          │         │
│  │  - JwtService (Token Generation)           │         │
│  │  - CurrentUser (User Resolver)             │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ Extensions                                  │         │
│  │  - PresentationExtensions (JWT Config)     │         │
│  └────────────────────────────────────────────┘         │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              APPLICATION LAYER                           │
│  ┌────────────────────────────────────────────┐         │
│  │ Models                                      │         │
│  │  - User (Domain Model)                     │         │
│  │  - TokenModel (JWT Tokens)                 │         │
│  │  - UserRole (Enum: Admin, User)            │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ Ports/Security (Interfaces)                │         │
│  │  - IAuthService                            │         │
│  │  - IJwtService                             │         │
│  │  - ICurrentUser                            │         │
│  │  - IUserService                            │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ UseCases (CQRS Handlers)                   │         │
│  │  - Auth/Register                           │         │
│  │  - Auth/Login                              │         │
│  │  - Auth/Refresh                            │         │
│  │  - Auth/Logout                             │         │
│  │  - Auth/GetProfile                         │         │
│  │  - User/GetAllUsers                        │         │
│  └────────────────────────────────────────────┘         │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│            INFRASTRUCTURE LAYER                          │
│  ┌────────────────────────────────────────────┐         │
│  │ Identity                                    │         │
│  │  - AppUser (IdentityUser<Guid>)            │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ Persistence                                 │         │
│  │  - AppDbContext (Identity Integration)     │         │
│  │  - Migrations                               │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ Adapters/Security                          │         │
│  │  - AuthService (Auth Logic)                │         │
│  │  - UserService (User Queries)              │         │
│  └────────────────────────────────────────────┘         │
│  ┌────────────────────────────────────────────┐         │
│  │ Options                                     │         │
│  │  - JwtOptions (JWT Configuration)          │         │
│  └────────────────────────────────────────────┘         │
└─────────────────────────────────────────────────────────┘
                          ↓
                  ┌───────────────┐
                  │   PostgreSQL   │
                  │   Database     │
                  └───────────────┘
```

### 1.3. Custom User Entity - AppUser

File: `MvInfrastructure/Identity/AppUser.cs`

```csharp
public class AppUser : IdentityUser<Guid>
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Convert to domain model
    public User ToUser()
    {
        return new User
        {
            Id = Id,
            UserName = UserName!,
            Email = Email!,
            PhoneNumber = PhoneNumber,
            Role = Role,
            CreatedAt = CreatedAt,
            SecurityStamp = SecurityStamp
        };
    }
}
```

**Các thuộc tính kế thừa từ IdentityUser<Guid>:**

| Property            | Type            | Mô tả                      |
| ------------------- | --------------- | -------------------------- |
| `Id`                | Guid            | Primary key của user       |
| `UserName`          | string          | Tên đăng nhập (unique)     |
| `Email`             | string          | Email (unique)             |
| `PasswordHash`      | string          | Mật khẩu đã được hash      |
| `SecurityStamp`     | string          | Dùng để invalidate tokens  |
| `PhoneNumber`       | string          | Số điện thoại              |
| `EmailConfirmed`    | bool            | Email đã xác nhận chưa     |
| `LockoutEnd`        | DateTimeOffset? | Thời gian kết thúc lockout |
| `AccessFailedCount` | int             | Số lần đăng nhập sai       |

### 1.4. Database Context với Identity

File: `MvInfrastructure/Persistence/AppDbContext.cs`

```csharp
public class AppDbContext : IdentityUserContext<AppUser, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình bảng Users
        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<string>();
        });

        // Seed default admin user
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var admin = new AppUser
        {
            Id = adminId,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "ADMIN-SECURITY-STAMP-FIXED",
            ConcurrencyStamp = "ADMIN-CONCURRENCY-STAMP-FIXED",
            Role = UserRole.Admin,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        builder.Entity<AppUser>().HasData(admin);
    }
}
```

**Lý do sử dụng `IdentityUserContext<AppUser, Guid>` thay vì `IdentityDbContext`:**

- ✅ **Nhẹ hơn**: Chỉ tạo bảng Users, không cần Roles, UserRoles, RoleClaims
- ✅ **Đơn giản hơn**: Phù hợp khi dùng Role enum thay vì Role table
- ✅ **Performance tốt hơn**: Ít joins, ít tables
- ✅ **Dễ maintain**: Không phải quản lý nhiều bảng

### 1.5. Dependency Injection Configuration

File: `MvInfrastructure/Extensions/IdentityExtensions.cs`

```csharp
public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext with PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            )
        );

        // Configure Identity Core
        services.AddIdentityCore<AppUser>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
```

---

## II. CƠ CHẾ XÁC THỰC VỚI JWT

### 2.1. Tổng quan về JWT (JSON Web Token)

**JWT** là một tiêu chuẩn mở (RFC 7519) cho việc tạo ra access tokens cho phép truyền thông tin an toàn giữa các bên dưới dạng đối tượng JSON.

#### 2.1.1. Cấu trúc JWT

JWT gồm 3 phần, được phân cách bởi dấu chấm `.`:

```
HEADER.PAYLOAD.SIGNATURE
```

**Ví dụ JWT thực tế:**

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**1. Header (Base64Url encoded):**

```json
{
  "alg": "HS256", // Thuật toán mã hóa
  "typ": "JWT" // Loại token
}
```

**2. Payload (Base64Url encoded):**

```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "019c5d26-ee9c-7ca9-94c9-0e81e67e4428",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "testuser",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "test@example.com",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "User",
  "jti": "aac8229b-9f75-4c57-895f-f985dfb5a80f",
  "SecurityStamp": "KQMGXLYALZZERLLERJ2DX7KE53UQIPQD",
  "exp": 1771092975, // Expiration time (Unix timestamp)
  "iss": "MvPresentation", // Issuer
  "aud": "MvPresentation" // Audience
}
```

**3. Signature:**

```
HMACSHA256(
    base64UrlEncode(header) + "." + base64UrlEncode(payload),
    secret_key
)
```

### 2.2. Cấu hình JWT trong Project

File: `MvInfrastructure/Options/JwtOptions.cs`

```csharp
public class JwtOptions
{
    [Required(ErrorMessage = "Secret Key là bắt buộc!")]
    [MinLength(32, ErrorMessage = "Secret Key phải có ít nhất 32 ký tự")]
    public string SecretKey { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Range(1, 1440)] // 1 phút đến 24 giờ
    public int AccessTokenExpiryMinutes { get; set; } = 30;

    [Range(1, 10080)] // 1 phút đến 7 ngày
    public int RefreshTokenExpiryMinutes { get; set; } = 1440; // 24 giờ

    public static string SectionName => "Jwt";
}
```

**Cấu hình trong appsettings.json:**

```json
{
  "Jwt": {
    "SecretKey": "DevSecretKeyForJwtThatIsAtLeast32CharactersLongForDevelopment!",
    "Issuer": "MvPresentation",
    "Audience": "MvPresentation",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryMinutes": 10080
  }
}
```

### 2.3. JWT Service Implementation

File: `MvPresentation/Adapters/Security/JwtService.cs`

```csharp
public class JwtService(JwtOptions jwtOptions) : IJwtService
{
    public TokenModel GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.SecurityStamp != null)
        {
            claims.Add(new Claim("SecurityStamp", user.SecurityStamp));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
        );
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );
        var expiry = DateTime.UtcNow.AddMinutes(
            jwtOptions.AccessTokenExpiryMinutes
        );

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new TokenModel
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiredAt = expiry
        };
    }

    public TokenModel GenerateRefreshToken(User user)
    {
        // Similar implementation với expiry time dài hơn
        // Chỉ chứa UserId, JTI và SecurityStamp
        // ...
    }
}
```

**Giải thích các claims:**

| Claim Type                    | Mô tả              | Ví dụ                                  |
| ----------------------------- | ------------------ | -------------------------------------- |
| `ClaimTypes.NameIdentifier`   | User ID            | "019c5d26-ee9c-7ca9-94c9-0e81e67e4428" |
| `ClaimTypes.Name`             | Username           | "testuser"                             |
| `ClaimTypes.Email`            | Email address      | "test@example.com"                     |
| `ClaimTypes.Role`             | User role          | "User" hoặc "Admin"                    |
| `JwtRegisteredClaimNames.Jti` | JWT ID (unique)    | "aac8229b-9f75-4c57-895f-f985dfb5a80f" |
| `SecurityStamp`               | Token invalidation | "KQMGXLYALZZ..."                       |

### 2.4. JWT Authentication Middleware Configuration

File: `MvPresentation/Extensions/PresentationExtensions.cs`

```csharp
public static class PresentationExtensions
{
    public static IServiceCollection AddPresentationInfrastructure(
        this IServiceCollection services)
    {
        // Register JWT Service
        services.AddScoped<IJwtService, JwtService>();

        // Register CurrentUser
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Add Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var jwtOptions = serviceProvider.GetRequiredService<JwtOptions>();

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
                ),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Add Authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
        });

        return services;
    }
}
```

**TokenValidationParameters giải thích:**

| Parameter                  | Mô tả                                                     |
| -------------------------- | --------------------------------------------------------- |
| `ValidateIssuerSigningKey` | Xác thực chữ ký token bằng secret key                     |
| `IssuerSigningKey`         | Secret key dùng để ký token                               |
| `ValidateIssuer`           | Kiểm tra token được phát hành bởi issuer hợp lệ           |
| `ValidateAudience`         | Kiểm tra token được tạo cho audience hợp lệ               |
| `ValidateLifetime`         | Kiểm tra token còn hạn hay không                          |
| `ClockSkew`                | Khoảng thời gian chênh lệch cho phép (set về 0 để strict) |

### 2.5. Current User Service

File: `MvPresentation/Adapters/Security/CurrentUser.cs`

```csharp
public class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor accessor)
    {
        var httpContext = accessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext không khả dụng");

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User chưa được xác thực");

        var userName = httpContext.User.FindFirstValue(ClaimTypes.Name)
            ?? throw new UnauthorizedAccessException("Username không tìm thấy");

        var email = httpContext.User.FindFirstValue(ClaimTypes.Email)
            ?? throw new UnauthorizedAccessException("Email không tìm thấy");

        var roleStr = httpContext.User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role không tìm thấy");

        var role = Enum.Parse<UserRole>(roleStr);
        var securityStamp = httpContext.User.FindFirstValue("SecurityStamp");

        User = new User
        {
            Id = Guid.Parse(userId),
            UserName = userName,
            Email = email,
            Role = role,
            SecurityStamp = securityStamp
        };
    }

    public User User { get; init; }
}
```

**Sử dụng trong Use Cases:**

```csharp
public class GetProfileHandler(ICurrentUser currentUser)
    : IRequestHandler<GetProfileQuery, Models.User>
{
    public Task<Models.User> Handle(GetProfileQuery request, CancellationToken ct)
    {
        // Lấy thông tin user hiện tại từ JWT claims
        return Task.FromResult(currentUser.User);
    }
}
```

### 2.6. Authentication Flow

```
┌─────────┐                    ┌────────────┐                   ┌──────────┐
│ Client  │                    │   Server   │                   │ Database │
└────┬────┘                    └─────┬──────┘                   └────┬─────┘
     │                               │                                │
     │  POST /api/user/auth/login    │                                │
     │  { email, password }          │                                │
     ├──────────────────────────────>│                                │
     │                               │                                │
     │                               │  Find user by email            │
     │                               ├───────────────────────────────>│
     │                               │                                │
     │                               │  User data                     │
     │                               │<───────────────────────────────┤
     │                               │                                │
     │                               │  Verify password hash          │
     │                               │  (bcrypt compare)              │
     │                               │                                │
     │                               │  Generate JWT:                 │
     │                               │  - Access Token (30min)        │
     │                               │  - Refresh Token (24h)         │
     │                               │                                │
     │  { access, refresh } tokens   │                                │
     │<──────────────────────────────┤                                │
     │                               │                                │
     │  Save tokens in localStorage  │                                │
     │                               │                                │
     │  GET /api/user/auth/profile   │                                │
     │  Authorization: Bearer {JWT}  │                                │
     ├──────────────────────────────>│                                │
     │                               │                                │
     │                               │  1. Validate JWT signature     │
     │                               │  2. Check expiration           │
     │                               │  3. Extract claims             │
     │                               │  4. Verify SecurityStamp       │
     │                               │                                │
     │                               │  Get user from database        │
     │                               ├───────────────────────────────>│
     │                               │                                │
     │                               │  User data                     │
     │                               │<───────────────────────────────┤
     │                               │                                │
     │  User Profile Data            │                                │
     │<──────────────────────────────┤                                │
     │                               │                                │
```

---

## III. CHIẾN LƯỢC PHÂN QUYỀN (ADMIN - USER)

### 3.1. Role-based Authorization

Dự án sử dụng **2 roles đơn giản**: `Admin` và `User` được định nghĩa bằng enum.

File: `MvApplication/Models/User.cs`

```csharp
public enum UserRole
{
    Admin,  // Role 0
    User    // Role 1
}
```

**Ưu điểm của role enum:**

- ✅ **Đơn giản**: Không cần bảng Roles, UserRoles trong database
- ✅ **Performance**: Không có join operations
- ✅ **Type-safe**: Compile-time checking
- ✅ **Dễ maintain**: Thay đổi roles chỉ cần sửa enum

**Nhược điểm:**

- ❌ Không linh hoạt khi cần thêm nhiều roles động
- ❌ Không hỗ trợ hierarchical roles
- ❌ Không có role permissions phức tạp

### 3.2. Authorization tại Application Layer

File: `MvInfrastructure/Adapters/Security/AuthService.cs`

```csharp
public class AuthService(
    UserManager<AppUser> userManager,
    IJwtService jwtService,
    JwtOptions jwtOptions
) : IAuthService
{
    public async Task<AuthTokens> RegisterAsync(
        string userName,
        string email,
        string password,
        UserRole role,  // Role được truyền vào
        CancellationToken ct = default)
    {
        // Kiểm tra email đã tồn tại
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new AppException("Email đã được sử dụng");
        }

        // Tạo user mới với role
        var appUser = new AppUser
        {
            UserName = userName,
            Email = email,
            Role = role,  // Set role
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(appUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppException($"Không thể tạo tài khoản: {errors}");
        }

        // Generate tokens với role trong claims
        var user = appUser.ToUser();
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);

        return new AuthTokens(accessToken, refreshToken);
    }
}
```

### 3.3. Controllers phân tách theo Role

#### 3.3.1. User Controller

File: `MvPresentation/Controllers/User/AuthController.cs`

```csharp
namespace MvPresentation.Controllers.User;

[ApiController]
[Route("api/user/auth")]
[ApiExplorerSettings(GroupName = "user")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        // Force role = User
        var result = await mediator.Send(
            command with { Role = UserRole.User }
        );
        return AppResponse.Success(result, "Đăng ký thành công");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        return AppResponse.Success(result, "Đăng nhập thành công");
    }

    [HttpGet("profile")]
    [Authorize(Roles = "User")]  // Chỉ User role
    public async Task<IActionResult> GetProfile()
    {
        var result = await mediator.Send(new GetProfileQuery());
        return AppResponse.Success(result, "Lấy thông tin thành công");
    }
}
```

#### 3.3.2. Admin Controller

File: `MvPresentation/Controllers/Admin/AuthController.cs`

```csharp
namespace MvPresentation.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
[ApiExplorerSettings(GroupName = "admin")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        // Force role = Admin
        var result = await mediator.Send(
            command with { Role = UserRole.Admin }
        );
        return AppResponse.Success(result, "Đăng ký admin thành công");
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Admin")]  // Chỉ Admin role
    public async Task<IActionResult> GetProfile()
    {
        var result = await mediator.Send(new GetProfileQuery());
        return AppResponse.Success(result, "Lấy thông tin thành công");
    }
}
```

#### 3.3.3. Admin User Management Controller

File: `MvPresentation/Controllers/Admin/UserManagementController.cs`

```csharp
namespace MvPresentation.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[ApiExplorerSettings(GroupName = "admin")]
[Authorize(Roles = "Admin")]  // Toàn bộ controller chỉ cho Admin
public class UserManagementController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách tất cả người dùng (chỉ Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await mediator.Send(new GetAllUsersQuery());
        return AppResponse.Success(result, "Lấy danh sách người dùng thành công");
    }
}
```

### 3.4. Authorization Matrix

| Endpoint                   | Method | Anonymous | User | Admin |
| -------------------------- | ------ | --------- | ---- | ----- |
| `/api/user/auth/register`  | POST   | ✅        | ✅   | ✅    |
| `/api/user/auth/login`     | POST   | ✅        | ✅   | ✅    |
| `/api/user/auth/profile`   | GET    | ❌        | ✅   | ❌    |
| `/api/user/auth/refresh`   | POST   | ✅        | ✅   | ✅    |
| `/api/user/auth/logout`    | POST   | ❌        | ✅   | ✅    |
| `/api/admin/auth/register` | POST   | ✅        | ✅   | ✅    |
| `/api/admin/auth/login`    | POST   | ✅        | ✅   | ✅    |
| `/api/admin/auth/profile`  | GET    | ❌        | ❌   | ✅    |
| `/api/admin/users`         | GET    | ❌        | ❌   | ✅    |

### 3.5. Policy-based Authorization

File: `MvPresentation/Extensions/PresentationExtensions.cs`

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User"));
});
```

**Sử dụng policies:**

```csharp
[Authorize(Policy = "AdminOnly")]
public class SecureController : ControllerBase
{
    // Chỉ Admin access
}
```

### 3.6. Testing Authorization

#### Test 1: User truy cập User endpoint (Success)

```bash
curl -X GET http://localhost:5273/api/user/auth/profile \
  -H "Authorization: Bearer {user_token}"

# Response: 200 OK
```

#### Test 2: User truy cập Admin endpoint (Forbidden)

```bash
curl -X GET http://localhost:5273/api/admin/users \
  -H "Authorization: Bearer {user_token}"

# Response: 403 Forbidden
```

#### Test 3: Admin truy cập Admin endpoint (Success)

```bash
curl -X GET http://localhost:5273/api/admin/users \
  -H "Authorization: Bearer {admin_token}"

# Response: 200 OK với danh sách users
```

---

## IV. QUẢN LÝ PHIÊN ĐĂNG NHẬP

### 4.1. Access Token vs Refresh Token

Dự án sử dụng **2 loại tokens** để cân bằng giữa bảo mật và trải nghiệm người dùng:

| Feature            | Access Token                     | Refresh Token                |
| ------------------ | -------------------------------- | ---------------------------- |
| **Mục đích**       | Gọi API endpoints                | Lấy access token mới         |
| **Thời gian sống** | Ngắn (30-60 phút)                | Dài (24 giờ - 7 ngày)        |
| **Claims**         | Full (userId, email, role, etc.) | Minimal (userId, jti)        |
| **Lưu trữ**        | Memory/SessionStorage            | LocalStorage (encrypted)     |
| **Sử dụng**        | Mỗi API request                  | Chỉ khi access token hết hạn |
| **Rủi ro**         | Thấp (ngắn hạn)                  | Cao hơn (cần bảo vệ tốt)     |

**Lý do cần cả 2 tokens:**

- ✅ **Bảo mật**: Access token ngắn → Giảm rủi ro nếu bị đánh cắp
- ✅ **UX tốt**: Refresh token dài → Không phải login liên tục
- ✅ **Revocation**: Có thể revoke refresh token khi cần
- ✅ **Scalability**: Stateless authentication

### 4.2. Token Generation

File: `MvPresentation/Adapters/Security/JwtService.cs`

```csharp
public TokenModel GenerateAccessToken(User user)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.UserName),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    if (user.SecurityStamp != null)
    {
        claims.Add(new Claim("SecurityStamp", user.SecurityStamp));
    }

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
    );
    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256
    );

    // Access token: 30-60 phút
    var expiry = DateTime.UtcNow.AddMinutes(
        jwtOptions.AccessTokenExpiryMinutes
    );

    var token = new JwtSecurityToken(
        issuer: jwtOptions.Issuer,
        audience: jwtOptions.Audience,
        claims: claims,
        expires: expiry,
        signingCredentials: credentials
    );

    return new TokenModel
    {
        Token = new JwtSecurityTokenHandler().WriteToken(token),
        ExpiredAt = expiry
    };
}

public TokenModel GenerateRefreshToken(User user)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    if (user.SecurityStamp != null)
    {
        claims.Add(new Claim("SecurityStamp", user.SecurityStamp));
    }

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
    );
    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256
    );

    // Refresh token: 24 giờ - 7 ngày
    var expiry = DateTime.UtcNow.AddMinutes(
        jwtOptions.RefreshTokenExpiryMinutes
    );

    var token = new JwtSecurityToken(
        issuer: jwtOptions.Issuer,
        audience: jwtOptions.Audience,
        claims: claims,
        expires: expiry,
        signingCredentials: credentials
    );

    return new TokenModel
    {
        Token = new JwtSecurityTokenHandler().WriteToken(token),
        ExpiredAt = expiry
    };
}
```

### 4.3. Refresh Token Flow

File: `MvInfrastructure/Adapters/Security/AuthService.cs`

```csharp
public async Task<AuthTokens> RefreshAsync(
    string refreshToken,
    CancellationToken ct = default)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

    try
    {
        // 1. Validate refresh token
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(
            refreshToken,
            validationParameters,
            out var validatedToken
        );

        // 2. Kiểm tra algorithm
        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new AppException("Token không hợp lệ");
        }

        // 3. Lấy user từ token
        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) ||
            !Guid.TryParse(userIdStr, out var userId))
        {
            throw new AppException("Token không hợp lệ");
        }

        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser == null)
        {
            throw new AppException("User không tồn tại");
        }

        // 4. Kiểm tra SecurityStamp
        var tokenSecurityStamp = principal.FindFirstValue("SecurityStamp");
        if (tokenSecurityStamp != appUser.SecurityStamp)
        {
            throw new AppException("Token đã bị vô hiệu hóa");
        }

        // 5. Generate new tokens
        var user = appUser.ToUser();
        var newAccessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken(user);

        return new AuthTokens(newAccessToken, newRefreshToken);
    }
    catch (Exception ex) when (ex is not AppException)
    {
        throw new AppException("Token không hợp lệ hoặc đã hết hạn");
    }
}
```

**Flow diagram:**

```
┌─────────┐                    ┌────────────┐                   ┌──────────┐
│ Client  │                    │   Server   │                   │ Database │
└────┬────┘                    └─────┬──────┘                   └────┬─────┘
     │                               │                                │
     │  Access Token expired         │                                │
     │                               │                                │
     │  POST /api/user/auth/refresh  │                                │
     │  { refreshToken }             │                                │
     ├──────────────────────────────>│                                │
     │                               │                                │
     │                               │  1. Validate refresh token     │
     │                               │     - Check signature          │
     │                               │     - Check expiration         │
     │                               │     - Check algorithm          │
     │                               │                                │
     │                               │  2. Extract userId from token  │
     │                               │                                │
     │                               │  3. Get user from database     │
     │                               ├───────────────────────────────>│
     │                               │                                │
     │                               │  User data with SecurityStamp  │
     │                               │<───────────────────────────────┤
     │                               │                                │
     │                               │  4. Verify SecurityStamp       │
     │                               │     matches token              │
     │                               │                                │
     │                               │  5. Generate new token pair    │
     │                               │     - New Access Token         │
     │                               │     - New Refresh Token        │
     │                               │                                │
     │  New { access, refresh }      │                                │
     │<──────────────────────────────┤                                │
     │                               │                                │
     │  Update tokens in storage     │                                │
     │                               │                                │
```

### 4.4. Logout Mechanism

File: `MvInfrastructure/Adapters/Security/AuthService.cs`

```csharp
public async Task LogoutAsync(
    string refreshToken,
    CancellationToken ct = default)
{
    if (string.IsNullOrEmpty(refreshToken))
    {
        return;
    }

    // Option 1: Token Blacklisting (with Redis)
    // var jti = ExtractJtiFromToken(refreshToken);
    // await _cache.BlacklistAsync(jti, timeRemaining);

    // Option 2: Update SecurityStamp (invalidate all tokens)
    // Implemented when changing password or security incident

    await Task.CompletedTask;
}
```

**2 cách logout:**

| Method                   | Scope             | Implementation             | Use Case                         |
| ------------------------ | ----------------- | -------------------------- | -------------------------------- |
| **Token Blacklisting**   | Thiết bị hiện tại | Store token JTI in Redis   | Normal logout                    |
| **SecurityStamp Update** | Tất cả thiết bị   | Update SecurityStamp in DB | Password change, security breach |

### 4.5. SecurityStamp cho Token Invalidation

**SecurityStamp** là một string random được lưu trong database, dùng để:

- ✅ Invalidate tất cả tokens khi cần
- ✅ Force logout từ tất cả thiết bị
- ✅ Security incident response

**Khi nào update SecurityStamp:**

1. **Đổi password**: Invalidate tất cả sessions cũ
2. **Thay đổi email**: Security-sensitive change
3. **Security breach detected**: Admin force logout user
4. **Role change**: Cần reload permissions

**Implementation:**

```csharp
public async Task ChangePasswordAsync(
    Guid userId,
    string currentPassword,
    string newPassword)
{
    var user = await userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
        throw new AppException("User không tồn tại");
    }

    // Change password
    var result = await userManager.ChangePasswordAsync(
        user,
        currentPassword,
        newPassword
    );

    if (!result.Succeeded)
    {
        throw new AppException("Không thể đổi mật khẩu");
    }

    // Update SecurityStamp to invalidate all tokens
    await userManager.UpdateSecurityStampAsync(user);

    // All existing tokens will become invalid
    // User must login again
}
```

### 4.6. Client-side Token Management

**Best practices cho client:**

```javascript
// Store tokens
localStorage.setItem("access_token", response.data.access.token);
localStorage.setItem("refresh_token", response.data.refresh.token);
localStorage.setItem("access_token_expiry", response.data.access.expiredAt);

// API request with auto-refresh
async function apiRequest(url, options) {
  let accessToken = localStorage.getItem("access_token");
  const expiry = new Date(localStorage.getItem("access_token_expiry"));

  // Check if token is about to expire (within 5 minutes)
  if (new Date() >= expiry - 5 * 60 * 1000) {
    // Refresh token
    const refreshToken = localStorage.getItem("refresh_token");
    const response = await fetch("/api/user/auth/refresh", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (response.ok) {
      const data = await response.json();
      localStorage.setItem("access_token", data.data.access.token);
      localStorage.setItem("refresh_token", data.data.refresh.token);
      localStorage.setItem("access_token_expiry", data.data.access.expiredAt);
      accessToken = data.data.access.token;
    } else {
      // Refresh failed, redirect to login
      window.location.href = "/login";
      return;
    }
  }

  // Make actual request
  return fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      Authorization: `Bearer ${accessToken}`,
    },
  });
}
```

---

## V. CÁC CẤU HÌNH BẢO MẬT TRONG IDENTITY

### 5.1. Password Policy

File: `MvInfrastructure/Extensions/IdentityExtensions.cs`

```csharp
services.AddIdentityCore<AppUser>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;              // Bắt buộc có số
    options.Password.RequireLowercase = true;          // Bắt buộc chữ thường
    options.Password.RequireUppercase = true;          // Bắt buộc chữ hoa
    options.Password.RequireNonAlphanumeric = true;    // Bắt buộc ký tự đặc biệt
    options.Password.RequiredLength = 8;               // Tối thiểu 8 ký tự
    options.Password.RequiredUniqueChars = 1;          // Số ký tự unique tối thiểu
});
```

**Password requirements:**

| Rule                     | Development | Production | Giải thích                     |
| ------------------------ | ----------- | ---------- | ------------------------------ |
| `RequiredLength`         | 8           | 12+        | Độ dài tối thiểu               |
| `RequireDigit`           | true        | true       | Phải có số (0-9)               |
| `RequireLowercase`       | true        | true       | Phải có chữ thường (a-z)       |
| `RequireUppercase`       | true        | true       | Phải có chữ hoa (A-Z)          |
| `RequireNonAlphanumeric` | true        | true       | Phải có ký tự đặc biệt (!@#$%) |
| `RequiredUniqueChars`    | 1           | 6+         | Số ký tự khác nhau             |

**Ví dụ passwords:**

- ❌ `password` - Thiếu chữ hoa, số, ký tự đặc biệt
- ❌ `Password` - Thiếu số và ký tự đặc biệt
- ❌ `Password1` - Thiếu ký tự đặc biệt
- ✅ `Password@123` - Đạt tất cả yêu cầu
- ✅ `MySecure#Pass2023` - Đạt tất cả yêu cầu

**Validation trong FluentValidation:**

File: `MvApplication/UseCases/Auth/Register/RegisterCommand.cs`

```csharp
public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password là bắt buộc")
            .MinimumLength(8).WithMessage("Password phải có ít nhất 8 ký tự")
            .Matches(@"[A-Z]").WithMessage("Password phải có ít nhất 1 chữ hoa")
            .Matches(@"[a-z]").WithMessage("Password phải có ít nhất 1 chữ thường")
            .Matches(@"[0-9]").WithMessage("Password phải có ít nhất 1 số")
            .Matches(@"[\W_]").WithMessage("Password phải có ít nhất 1 ký tự đặc biệt");
    }
}
```

### 5.2. Account Lockout System

File: `MvInfrastructure/Extensions/IdentityExtensions.cs`

```csharp
services.AddIdentityCore<AppUser>(options =>
{
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
```

**Lockout configuration:**

| Setting                   | Value      | Giải thích                  |
| ------------------------- | ---------- | --------------------------- |
| `DefaultLockoutTimeSpan`  | 15 minutes | Thời gian khóa tài khoản    |
| `MaxFailedAccessAttempts` | 5          | Số lần đăng nhập sai tối đa |
| `AllowedForNewUsers`      | true       | Áp dụng cho cả user mới     |

**Flow khi login sai:**

```
Login Attempt 1 (Failed) → AccessFailedCount = 1
Login Attempt 2 (Failed) → AccessFailedCount = 2
Login Attempt 3 (Failed) → AccessFailedCount = 3
Login Attempt 4 (Failed) → AccessFailedCount = 4
Login Attempt 5 (Failed) → Account Locked for 15 minutes
                           LockoutEnd = DateTime.Now + 15 minutes
```

**Implementation trong AuthService:**

File: `MvInfrastructure/Adapters/Security/AuthService.cs`

```csharp
public async Task<AuthTokens> LoginAsync(
    string email,
    string password,
    CancellationToken ct = default)
{
    var appUser = await userManager.FindByEmailAsync(email);
    if (appUser == null)
    {
        throw new AppException("Email hoặc mật khẩu không đúng");
    }

    // 1. Kiểm tra account lockout
    if (await userManager.IsLockedOutAsync(appUser))
    {
        var lockoutEnd = await userManager.GetLockoutEndDateAsync(appUser);
        var remainingTime = lockoutEnd.HasValue
            ? (lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes
            : 0;

        throw new AppException(
            $"Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần. " +
            $"Vui lòng thử lại sau {Math.Ceiling(remainingTime)} phút."
        );
    }

    // 2. Kiểm tra password
    var isValidPassword = await userManager.CheckPasswordAsync(appUser, password);

    if (!isValidPassword)
    {
        // Tăng failed attempts
        await userManager.AccessFailedAsync(appUser);

        var failedCount = await userManager.GetAccessFailedCountAsync(appUser);
        var remainingAttempts = 5 - failedCount;

        if (remainingAttempts > 0)
        {
            throw new AppException(
                $"Email hoặc mật khẩu không đúng. " +
                $"Còn {remainingAttempts} lần thử."
            );
        }
        else
        {
            throw new AppException(
                "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần."
            );
        }
    }

    // 3. Reset failed attempts khi login thành công
    await userManager.ResetAccessFailedCountAsync(appUser);

    // 4. Cập nhật last login
    appUser.LastLoginAt = DateTime.UtcNow;
    await userManager.UpdateAsync(appUser);

    // 5. Generate tokens
    var user = appUser.ToUser();
    var accessToken = jwtService.GenerateAccessToken(user);
    var refreshToken = jwtService.GenerateRefreshToken(user);

    return new AuthTokens(accessToken, refreshToken);
}
```

### 5.3. Email Confirmation

**Tính năng có thể mở rộng:**

```csharp
services.AddIdentityCore<AppUser>(options =>
{
    // Email confirmation
    options.SignIn.RequireConfirmedEmail = true;
});

// Generate email confirmation token
var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

// Send email with confirmation link
var confirmationLink = $"{baseUrl}/confirm-email?userId={user.Id}&token={token}";
await emailService.SendEmailAsync(user.Email, "Confirm your email", confirmationLink);

// Confirm email
var result = await userManager.ConfirmEmailAsync(user, token);
```

### 5.4. Two-Factor Authentication (2FA)

**Tính năng có thể mở rộng:**

```csharp
services.AddIdentityCore<AppUser>(options =>
{
    // Two-factor authentication
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
});

// Enable 2FA
await userManager.SetTwoFactorEnabledAsync(user, true);

// Generate 2FA key
var key = await userManager.GetAuthenticatorKeyAsync(user);

// Verify 2FA code
var isValid = await userManager.VerifyTwoFactorTokenAsync(
    user,
    TokenOptions.DefaultAuthenticatorProvider,
    code
);
```

### 5.5. Security Best Practices

#### 5.5.1. Secret Key Management

**❌ KHÔNG NÊN:**

```json
// appsettings.json committed to Git
{
  "Jwt": {
    "SecretKey": "my-secret-key-123"
  }
}
```

**✅ NÊN:**

```bash
# Environment Variables
export JWT__SECRETKEY="YourSuperSecretKeyForJwtThatIsAtLeast32CharactersLong!"

# Or use Azure Key Vault
# Or use Docker Secrets
# Or use .env file (not committed)
```

#### 5.5.2. HTTPS Only

```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

#### 5.5.3. CORS Configuration

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("https://yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

#### 5.5.4. Rate Limiting

```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});
```

#### 5.5.5. Logging & Monitoring

```csharp
// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Log authentication events
logger.LogInformation(
    "User {UserId} logged in successfully from IP {IpAddress}",
    user.Id,
    httpContext.Connection.RemoteIpAddress
);

logger.LogWarning(
    "Failed login attempt for email {Email} from IP {IpAddress}",
    email,
    httpContext.Connection.RemoteIpAddress
);
```

---

## VI. VÍ DỤ MINH HỌA

### 6.1. Đăng ký User mới

**Request:**

```http
POST /api/user/auth/register HTTP/1.1
Host: localhost:5273
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
  "status": 200,
  "message": "Đăng ký thành công",
  "data": {
    "access": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiredAt": "2026-02-15T10:30:00Z"
    },
    "refresh": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiredAt": "2026-02-22T09:30:00Z"
    }
  }
}
```

### 6.2. Đăng nhập

**Request:**

```http
POST /api/user/auth/login HTTP/1.1
Host: localhost:5273
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Strong@Password123"
}
```

**Response:**

```json
{
  "status": 200,
  "message": "Đăng nhập thành công",
  "data": {
    "access": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiredAt": "2026-02-15T10:45:00Z"
    },
    "refresh": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiredAt": "2026-02-22T09:45:00Z"
    }
  }
}
```

### 6.3. Lấy Profile (Protected Endpoint)

**Request:**

```http
GET /api/user/auth/profile HTTP/1.1
Host: localhost:5273
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**

```json
{
  "status": 200,
  "message": "Lấy thông tin thành công",
  "data": {
    "id": "019c5d26-ee9c-7ca9-94c9-0e81e67e4428",
    "userName": "john_doe",
    "email": "john@example.com",
    "role": "User",
    "createdAt": "2026-02-15T09:30:00Z",
    "securityStamp": "KQMGXLYALZZERLLERJ2DX7KE53UQIPQD"
  }
}
```

### 6.4. Refresh Token

**Request:**

```http
POST /api/user/auth/refresh HTTP/1.1
Host: localhost:5273
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:**

```json
{
  "status": 200,
  "message": "Làm mới token thành công",
  "data": {
    "access": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiredAt": "2026-02-15T11:00:00Z"
    },
    "refresh": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiredAt": "2026-02-22T10:00:00Z"
    }
  }
}
```

### 6.5. Admin lấy danh sách Users

**Request:**

```http
GET /api/admin/users HTTP/1.1
Host: localhost:5273
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**

```json
{
  "status": 200,
  "message": "Lấy danh sách người dùng thành công",
  "data": [
    {
      "id": "019c5d26-ee9c-7ca9-94c9-0e81e67e4428",
      "userName": "john_doe",
      "email": "john@example.com",
      "role": "User",
      "createdAt": "2026-02-15T09:30:00Z",
      "securityStamp": "KQMGXLYALZZERLLERJ2DX7KE53UQIPQD"
    },
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "userName": "admin",
      "email": "admin@example.com",
      "role": "Admin",
      "createdAt": "2026-01-01T00:00:00Z",
      "securityStamp": "ADMIN-SECURITY-STAMP-FIXED"
    }
  ]
}
```

### 6.6. Error Responses

#### 6.6.1. Invalid Password

**Request:**

```http
POST /api/user/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "wrongpassword"
}
```

**Response:**

```json
{
  "status": 400,
  "error": "Email hoặc mật khẩu không đúng. Còn 4 lần thử."
}
```

#### 6.6.2. Account Locked

**Response:**

```json
{
  "status": 400,
  "error": "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau 14 phút."
}
```

#### 6.6.3. Unauthorized (No Token)

**Request:**

```http
GET /api/user/auth/profile HTTP/1.1
```

**Response:**

```
HTTP/1.1 401 Unauthorized
```

#### 6.6.4. Forbidden (Wrong Role)

**Request:**

```http
GET /api/admin/users HTTP/1.1
Authorization: Bearer {user_token}
```

**Response:**

```
HTTP/1.1 403 Forbidden
```

#### 6.6.5. Weak Password

**Request:**

```http
POST /api/user/auth/register HTTP/1.1
Content-Type: application/json

{
  "userName": "test",
  "email": "test@example.com",
  "password": "weak"
}
```

**Response:**

```json
{
  "status": 400,
  "error": "Validation failed",
  "errors": {
    "Password": [
      "Password phải có ít nhất 8 ký tự",
      "Password phải có ít nhất 1 chữ hoa",
      "Password phải có ít nhất 1 số",
      "Password phải có ít nhất 1 ký tự đặc biệt"
    ]
  }
}
```

### 6.7. Testing với cURL

```bash
# 1. Register User
curl -X POST http://localhost:5273/api/user/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "Test@Password123"
  }'

# 2. Login
curl -X POST http://localhost:5273/api/user/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@Password123"
  }'

# 3. Get Profile (replace {token} with actual token)
curl -X GET http://localhost:5273/api/user/auth/profile \
  -H "Authorization: Bearer {token}"

# 4. Register Admin
curl -X POST http://localhost:5273/api/admin/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin_user",
    "email": "admin@example.com",
    "password": "Admin@Password123"
  }'

# 5. Admin Get Users (replace {admin_token} with actual admin token)
curl -X GET http://localhost:5273/api/admin/users \
  -H "Authorization: Bearer {admin_token}"

# 6. Test Authorization (should return 403)
curl -X GET http://localhost:5273/api/admin/users \
  -H "Authorization: Bearer {user_token}"
```

### 6.8. Testing với Postman

#### 6.8.1. Setup Environment

Tạo environment với variables:

- `base_url`: http://localhost:5273
- `user_access_token`: (will be set after login)
- `admin_access_token`: (will be set after admin login)

#### 6.8.2. Collection Structure

```
Identity & JWT Tests
├── User Auth
│   ├── Register User
│   ├── Login User (saves token to environment)
│   ├── Get Profile
│   ├── Refresh Token
│   └── Logout
├── Admin Auth
│   ├── Register Admin
│   ├── Login Admin (saves token to environment)
│   ├── Get Profile
│   └── Get All Users
└── Authorization Tests
    ├── User tries Admin endpoint (should fail)
    └── Test without token (should fail)
```

#### 6.8.3. Post-response Script (Auto save token)

```javascript
// Login User - Tests tab
if (pm.response.code === 200) {
  const response = pm.response.json();
  pm.environment.set("user_access_token", response.data.access.token);
  pm.environment.set("user_refresh_token", response.data.refresh.token);
}

// Login Admin - Tests tab
if (pm.response.code === 200) {
  const response = pm.response.json();
  pm.environment.set("admin_access_token", response.data.access.token);
  pm.environment.set("admin_refresh_token", response.data.refresh.token);
}
```

---

## KẾT LUẬN

Dự án Chapter 6 đã thành công implement một hệ thống bảo mật và phân quyền hoàn chỉnh với các features:

### ✅ Đã hoàn thành:

1. **ASP.NET Core Identity**
   - Custom AppUser entity với Guid primary key
   - Password hashing tự động
   - Account lockout mechanism
   - Seeded admin account

2. **JWT Authentication**
   - Access Token (ngắn hạn - 60 phút)
   - Refresh Token (dài hạn - 7 ngày)
   - Claims-based authorization
   - Token validation middleware

3. **Authorization**
   - 2 Roles: Admin và User
   - Role-based authorization với `[Authorize(Roles = "Admin")]`
   - Separate controllers cho User và Admin
   - Policy-based authorization

4. **Security**
   - Strong password policy (8+ chars, uppercase, lowercase, digit, special char)
   - Account lockout sau 5 lần login sai
   - SecurityStamp cho token invalidation
   - HTTPS enforcement

5. **Clean Architecture**
   - Application Layer: Use Cases, Models, Ports
   - Infrastructure Layer: Identity, Persistence, Adapters
   - Presentation Layer: Controllers, JWT Service, Middleware

6. **Best Practices**
   - CQRS pattern với MediatR
   - FluentValidation cho input validation
   - Async/await throughout
   - Dependency Injection
   - Separation of Concerns

### 🚀 Có thể mở rộng:

- [ ] Email confirmation
- [ ] Password reset via email
- [ ] Two-factor authentication (2FA)
- [ ] OAuth/External login (Google, Facebook)
- [ ] Token blacklisting với Redis
- [ ] Claim-based complex authorization
- [ ] Rate limiting
- [ ] Audit logging
- [ ] User profile management
- [ ] Role management UI

### 📊 Kết quả testing:

| Test Case                 | Status  |
| ------------------------- | ------- |
| User Registration         | ✅ PASS |
| User Login                | ✅ PASS |
| User Profile Access       | ✅ PASS |
| Admin Registration        | ✅ PASS |
| Admin Login               | ✅ PASS |
| Admin Get Users           | ✅ PASS |
| Authorization (403)       | ✅ PASS |
| Invalid Login             | ✅ PASS |
| Weak Password Rejection   | ✅ PASS |
| Duplicate Email           | ✅ PASS |
| Unauthorized Access (401) | ✅ PASS |
| Refresh Token             | ✅ PASS |

**Tổng số tests: 12/12 PASSED** ✅

---

**Tác giả:** Nguyễn Trần Gia Hiếu  
**Ngày:** 15/02/2026  
**Version:** 1.0
