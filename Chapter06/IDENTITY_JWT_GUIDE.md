# CHƯƠNG 6: BẢO MẬT VÀ PHÂN QUYỀN DỰA TRÊN JWT VÀ IDENTITY

Tài liệu này chứa hướng dẫn chi tiết về thiết lập hệ thống bảo mật, xác thực người dùng và phân quyền truy cập trong ứng dụng ASP.NET Core sử dụng JWT và thư viện Identity.

---

## MỤC LỤC

1. [Hệ thống danh tính (Identity System)](#1-hệ-thống-danh-tính-identity-system)
2. [Xác thực với JSON Web Token (JWT)](#2-xác-thực-với-json-web-token-jwt)
3. [Chiến lược phân quyền (Authorization)](#3-chiến-lược-phân-quyền-authorization)
4. [Quản lý phiên đăng nhập và Refresh Token](#4-quản-lý-phiên-đăng-nhập-và-refresh-token)
5. [Cấu hình bảo mật và Identity Options](#5-cấu-hình-bảo-mật-và-identity-options)
6. [Kiến trúc tổng thể](#6-kiến-trúc-tổng-thể)
7. [Flows xác thực](#7-flows-xác-thực)

---

## 1. HỆ THỐNG DANH TÍNH (Identity System)

### 1.1 Tổng quan

ASP.NET Core Identity là framework quản lý người dùng tích hợp sẵn, cung cấp:

- Quản lý tài khoản người dùng (Users)
- Quản lý vai trò (Roles)
- Quản lý Claims
- Password hashing
- Token generation cho password reset, email confirmation
- Account lockout
- Two-factor authentication

### 1.2 Cấu hình Identity Core

**File: `L3.Infrastructure/Extensions/IdentityExtensions.cs`**

```csharp
using L2.Application.Ports.Security;
using L3.Infrastructure.Adapters.Security;
using L3.Infrastructure.Identity;
using L3.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace L3.Infrastructure.Extensions;

public static class IdentityExtensions {
  public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services) {
    // Identity Core Configuration
    services.AddIdentityCore<AppUser>(options => {
        // Password Policy
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        // User settings
        options.User.RequireUniqueEmail = true;

        // Lockout settings (optional, can be configured)
        // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        // options.Lockout.MaxFailedAccessAttempts = 5;
        // options.Lockout.AllowedForNewUsers = true;
      })
      .AddEntityFrameworkStores<AppDbContext>()  // Lưu trữ trong database
      .AddDefaultTokenProviders();  // Token providers cho password reset

    // Đăng ký các service implementations
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IUserService, UserService>();

    return services;
  }
}
```

**Giải thích:**

- **`AddIdentityCore<AppUser>`**: Đăng ký Identity với custom user entity
- **`AddEntityFrameworkStores<AppDbContext>`**: Sử dụng Entity Framework để lưu trữ
- **`AddDefaultTokenProviders()`**: Cung cấp token cho password reset, email confirmation

**So sánh `AddIdentityCore` vs `AddIdentity`:**

| Feature                   | AddIdentityCore  | AddIdentity             |
| ------------------------- | ---------------- | ----------------------- |
| **Use Case**              | API applications | MVC applications với UI |
| **Cookie Authentication** | ❌ Không         | ✅ Có                   |
| **SignInManager**         | ❌ Không         | ✅ Có                   |
| **Lightweight**           | ✅ Yes           | Heavier                 |
| **Perfect for JWT**       | ✅ Yes           | Overkill                |

### 1.3 Custom User Entity

**File: `L3.Infrastructure/Identity/AppUser.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using L2.Application.Models;
using Microsoft.AspNetCore.Identity;

namespace L3.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid> {
  // Custom properties
  [MaxLength(100)]
  public string FullName { get; set; } = null!;

  [MaxLength(255)]
  public string? Url { get; set; }

  public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;

  public DateTime? DeletedAt { get; private set; }

  public bool IsDeleted { get; private set; }

  // Role as enum (simple approach)
  public UserRole Role { get; init; } = UserRole.Bidder;

  // Business methods
  public void Update(string fullName, string? url) {
    FullName = fullName;
    Url = url;
  }

  public void Delete() {
    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
  }

  public void Restore() {
    IsDeleted = false;
    DeletedAt = null;
  }
}
```

**Properties kế thừa từ `IdentityUser<Guid>`:**

- `Id` (Guid)
- `UserName` (string)
- `Email` (string)
- `PasswordHash` (string)
- `SecurityStamp` (string) - **Quan trọng cho token invalidation**
- `PhoneNumber` (string)
- `EmailConfirmed` (bool)
- `LockoutEnd` (DateTimeOffset?) - Cho account lockout
- `AccessFailedCount` (int)

### 1.4 DbContext Integration

**File: `L3.Infrastructure/Persistence/AppDbContext.cs`**

```csharp
using L3.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace L3.Infrastructure.Persistence;

public class AppDbContext : IdentityUserContext<AppUser, Guid> {
  public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

  // Domain entities
  public DbSet<Auction> Auctions => Set<Auction>();
  public DbSet<Bid> Bids => Set<Bid>();
  // ... other domain entities

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    base.OnModelCreating(modelBuilder);

    // Configure Role as string enum
    modelBuilder.Entity<AppUser>()
      .Property(u => u.Role)
      .HasConversion<string>();

    // Apply other configurations
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  }
}
```

**Chú ý:**

- Sử dụng `IdentityUserContext<AppUser, Guid>` thay vì `IdentityDbContext` khi chỉ cần quản lý Users
- `IdentityUserContext` chỉ tạo bảng Users, không tạo bảng Roles, UserRoles, RoleClaims
- Phù hợp khi sử dụng Role đơn giản (enum)

### 1.5 Application Layer Models

**File: `L2.Application/Models/User.cs`**

```csharp
namespace L2.Application.Models;

public record User {
  public Guid Id { get; init; } = Guid.NewGuid();
  public string FullName { get; init; } = null!;
  public string Email { get; init; } = null!;
  public string? PhoneNumber { get; init; }
  public string? Url { get; init; }
  public bool IsActive { get; init; } = true;
  public UserRole Role { get; init; }
  public string? SecurityStamp { get; set; }  // Cần thiết cho token validation
}

public enum UserRole {
  Admin,
  Bidder
}
```

**Tại sao cần model riêng?**

- ✅ Tách biệt Infrastructure concerns khỏi Application layer
- ✅ Chỉ expose properties cần thiết
- ✅ Dễ dàng serialize/deserialize
- ✅ Không bị ràng buộc bởi Entity Framework

---

## 2. XÁC THỰC VỚI JSON WEB TOKEN (JWT)

### 2.1 JWT là gì?

**JSON Web Token (JWT)** là một chuẩn mở (RFC 7519) định nghĩa cách truyền thông tin an toàn giữa các bên dưới dạng JSON object.

**Cấu trúc JWT:**

```
xxxxx.yyyyy.zzzzz
  |     |     |
Header Payload Signature
```

**Ví dụ JWT:**

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**1. Header** (Base64Url encoded):

```json
{
  "alg": "HS256", // Algorithm
  "typ": "JWT" // Type
}
```

**2. Payload** (Base64Url encoded):

```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022,
  "exp": 1516242622
}
```

**3. Signature**:

```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

### 2.2 JWT Options Configuration

**File: `L3.Infrastructure/Options/JwtOptions.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace L3.Infrastructure.Options;

public class JwtOptions : IAppOptions {
  [Required(ErrorMessage = "Secret Key là bắt buộc!")]
  [MinLength(32, ErrorMessage = "Secret Key quá ngắn, không an toàn!")]
  public string Secret { get; set; } = string.Empty;

  public string Issuer { get; set; } = "BiddingOnlineAPI";
  public string Audience { get; set; } = "BiddingOnlineClient";

  [Range(1, int.MaxValue, ErrorMessage = "Thời gian hết hạn phải lớn hơn 0")]
  public int AccessExpiration { get; set; } = 60;  // 60 minutes

  [Range(1, int.MaxValue)]
  public int RefreshExpiration { get; set; } = 1440;  // 24 hours (1440 minutes)

  public static string SectionName => "Jwt";
}
```

**File: `appsettings.json`**

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "BiddingOnlineAPI",
    "Audience": "BiddingOnlineClient",
    "AccessExpiration": 60,
    "RefreshExpiration": 1440
  }
}
```

**Best Practices:**

- ✅ Secret key phải >= 256 bits (32 characters)
- ✅ Lưu Secret trong Environment Variables hoặc Azure Key Vault
- ✅ Không commit secret vào Git
- ✅ Access token nên ngắn (5-60 phút)
- ✅ Refresh token nên dài hơn (24 giờ - 7 ngày)

### 2.3 JWT Service Implementation

**File: `L0.API/Adapters/Security/JwtService.cs`**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using L2.Application.Models;
using L2.Application.Ports.Security;
using L3.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace L0.API.Adapters.Security;

public class JwtService(JwtOptions jwtOptions) : IJwtService {

  /// <summary>
  /// Tạo Access Token với đầy đủ thông tin user
  /// </summary>
  public TokenModel GenerateAccessToken(User user) {
    var claims = new List<Claim> {
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(ClaimTypes.Email, user.Email),
      new(ClaimTypes.Name, user.FullName),
      new(ClaimTypes.Role, nameof(user.Role)),
      new("security_stamp", user.SecurityStamp ?? ""),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())  // JWT ID
    };

    return GenerateToken(claims, jwtOptions.AccessExpiration);
  }

  /// <summary>
  /// Tạo Refresh Token với thông tin tối thiểu
  /// </summary>
  public TokenModel GenerateRefreshToken(User user) {
    var claims = new List<Claim> {
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new("security_stamp", user.SecurityStamp ?? ""),
      new("token_type", "refresh"),  // Đánh dấu loại token
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    return GenerateToken(claims, jwtOptions.RefreshExpiration);
  }

  /// <summary>
  /// Core method tạo JWT token
  /// </summary>
  private TokenModel GenerateToken(IEnumerable<Claim> claims, int expirationMinutes) {
    // 1. Tạo signing key
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // 2. Tính thời gian hết hạn
    var expiry = DateTime.UtcNow.AddMinutes(expirationMinutes);

    // 3. Tạo JWT token
    var token = new JwtSecurityToken(
      issuer: jwtOptions.Issuer,
      audience: jwtOptions.Audience,
      claims: claims,
      expires: expiry,
      signingCredentials: creds
    );

    // 4. Serialize token thành string
    return new TokenModel {
      Token = new JwtSecurityTokenHandler().WriteToken(token),
      ExpiredAt = expiry
    };
  }
}
```

**Interface: `L2.Application/Ports/Security/IJwtService.cs`**

```csharp
using L2.Application.Models;

namespace L2.Application.Ports.Security;

public interface IJwtService {
  TokenModel GenerateAccessToken(User user);
  TokenModel GenerateRefreshToken(User user);
}
```

**Model: `L2.Application/Models/TokenModel.cs`**

```csharp
namespace L2.Application.Models;

public record TokenModel {
  public string Token { get; init; } = null!;
  public DateTime ExpiredAt { get; init; }
}

public record AuthTokens(TokenModel Access, TokenModel Refresh);
```

### 2.4 JWT Authentication Middleware

**File: `L0.API/Extensions/PresentationExtensions.cs`**

```csharp
using System.Text;
using L0.API.Adapters.Security;
using L2.Application.Ports.Security;
using L3.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace L0.API.Extensions;

public static class PresentationExtensions {
  public static IServiceCollection AddApiAuthentication(this IServiceCollection services) {
    // Đăng ký services
    services.AddScoped<ICurrentUser, CurrentUser>();
    services.AddScoped<IJwtService, JwtService>();

    // Cấu hình JWT Bearer Authentication
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer();

    // Configure JWT Bearer Options
    services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsRef) => {
          var jwtOptions = jwtOptionsRef.Value;

          // Token validation parameters
          bearerOptions.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(jwtOptions.Secret)
            ),
            ClockSkew = TimeSpan.Zero  // Không cho phép thời gian lệch
          };

          // Custom events
          bearerOptions.Events = new JwtBearerEvents {
            // Hỗ trợ JWT qua query string cho SignalR
            OnMessageReceived = context => {
              var accessToken = context.Request.Query["access_token"];
              var path = context.HttpContext.Request.Path;

              if (!string.IsNullOrEmpty(accessToken) &&
                  path.StartsWithSegments("/hubs")) {
                context.Token = accessToken;
              }

              return Task.CompletedTask;
            },

            // Log authentication failures
            OnAuthenticationFailed = context => {
              Console.WriteLine($"Authentication failed: {context.Exception.Message}");
              return Task.CompletedTask;
            }
          };
        }
      );

    return services;
  }
}
```

**Đăng ký trong Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Presentation (bao gồm JWT Authentication)
builder.Services.AddPresentationInfrastructure();

var app = builder.Build();

// QUAN TRỌNG: Thứ tự middleware
app.UseAuthentication();  // Phải đứng trước Authorization
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### 2.5 Current User Service

**File: `L0.API/Adapters/Security/CurrentUser.cs`**

```csharp
using System.Security.Claims;
using L2.Application.Models;
using L2.Application.Ports.Security;

namespace L0.API.Adapters.Security;

public class CurrentUser : ICurrentUser {
  public CurrentUser(IHttpContextAccessor accessor) {
    var user = accessor.HttpContext?.User;

    if (user?.Identity?.IsAuthenticated == true) {
      var id = user.FindFirstValue(ClaimTypes.NameIdentifier);

      User = new User {
        Id = Guid.Parse(id!),
        Email = user.FindFirstValue(ClaimTypes.Email) ?? "",
        FullName = user.FindFirstValue(ClaimTypes.Name) ?? "",
        Role = user.FindFirstValue(ClaimTypes.Role) == nameof(UserRole.Admin)
          ? UserRole.Admin
          : UserRole.Bidder
      };
    } else {
      User = new User { FullName = "Guest" };
    }
  }

  public User User { get; init; }
}
```

**Sử dụng trong Use Cases:**

```csharp
public class GetProfileHandler(
  IUserService userService,
  ICurrentUser currentUser
) : IRequestHandler<GetProfileQuery, GetProfileResult> {

  public async Task<GetProfileResult> Handle(
    GetProfileQuery request,
    CancellationToken ct
  ) {
    // Lấy user ID từ JWT token
    var userId = currentUser.User.Id;

    // Query user info
    var user = await userService.GetByIdAsync(userId, ct);

    return new GetProfileResult(user);
  }
}
```

---

## 3. CHIẾN LƯỢC PHÂN QUYỀN (Authorization)

### 3.1 Role-based Authorization

Project sử dụng 2 roles đơn giản: **Admin** và **Bidder**.

**Ưu điểm:**

- ✅ Đơn giản, dễ implement
- ✅ Phù hợp với hệ thống nhỏ, roles ít
- ✅ Không cần bảng Roles, UserRoles

**Nhược điểm:**

- ❌ Không linh hoạt khi roles nhiều
- ❌ Khó scale khi cần hierarchical roles
- ❌ Không hỗ trợ dynamic roles

#### 3.1.1 Phân quyền tại Application Layer

**File: `L3.Infrastructure/Adapters/Security/AuthService.cs`**

```csharp
public class AuthService(
  UserManager<AppUser> userManager,
  IJwtService jwtService,
  // ... other dependencies
) : IAuthService {

  /// <summary>
  /// Login cho Bidder
  /// </summary>
  public async Task<AuthTokens> LoginUserAsync(
    string email,
    string password,
    CancellationToken ct
  ) {
    return await AuthenticateAsync(email, password, UserRole.Bidder);
  }

  /// <summary>
  /// Login cho Admin
  /// </summary>
  public async Task<AuthTokens> LoginAdminAsync(
    string email,
    string password,
    CancellationToken ct
  ) {
    return await AuthenticateAsync(email, password, UserRole.Admin);
  }

  /// <summary>
  /// Core authentication method với role checking
  /// </summary>
  private async Task<AuthTokens> AuthenticateAsync(
    string email,
    string password,
    UserRole role
  ) {
    var user = await userManager.FindByEmailAsync(email);

    // Validate user
    if (user == null || user.IsDeleted) {
      throw new AppException("Thông tin đăng nhập không chính xác", 401);
    }

    // Check role - QUAN TRỌNG
    if (user.Role != role) {
      throw new AppException("Thông tin đăng nhập không chính xác", 401);
    }

    // Validate password
    if (!await userManager.CheckPasswordAsync(user, password)) {
      throw new AppException("Thông tin đăng nhập không chính xác", 401);
    }

    // Check account lockout
    if (await userManager.IsLockedOutAsync(user)) {
      throw new AppException("Tài khoản đang bị khóa. Vui lòng liên hệ Admin.", 403);
    }

    // Generate tokens
    var userModel = ToUserModel(user);
    return new AuthTokens(
      jwtService.GenerateAccessToken(userModel),
      jwtService.GenerateRefreshToken(userModel)
    );
  }
}
```

#### 3.1.2 Separate Controllers

**User Controller (Bidder):**

```csharp
namespace L0.API.Controllers.Bidder;

[ApiController]
[Route("api/user/[controller]")]  // /api/user/auth
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController(IMediator mediator) : ControllerBase {

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginCommand command) {
    var result = await mediator.Send(command);
    return AppResponse.Success(result.Tokens, "Đăng nhập thành công");
  }
}
```

**Admin Controller:**

```csharp
namespace L0.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]  // /api/admin/auth
[ApiExplorerSettings(GroupName = "v2")]
public class AuthController(IMediator mediator) : ControllerBase {

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginCommand command) {
    var result = await mediator.Send(command);
    return AppResponse.Success(result.Tokens, "Đăng nhập thành công");
  }
}
```

### 3.2 Claim-based Authorization

Claims là các thông tin bổ sung về user (key-value pairs).

**Ví dụ claims trong JWT:**

```json
{
  "nameid": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Bidder",
  "security_stamp": "abc123",
  "jti": "unique-token-id"
}
```

**Sử dụng trong code:**

```csharp
// Lấy claim từ HttpContext
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var email = User.FindFirstValue(ClaimTypes.Email);
var role = User.FindFirstValue(ClaimTypes.Role);
```

### 3.3 Policy-based Authorization

**Ví dụ cấu hình policies:**

```csharp
// Program.cs
builder.Services.AddAuthorization(options => {
  // Policy: Chỉ Admin mới được truy cập
  options.AddPolicy("AdminOnly", policy =>
    policy.RequireRole("Admin"));

  // Policy: Requirement phức tạp hơn
  options.AddPolicy("BidderWithEmail", policy =>
    policy.RequireRole("Bidder")
          .RequireClaim(ClaimTypes.Email));

  // Policy: Custom requirement
  options.AddPolicy("MinimumAge", policy =>
    policy.Requirements.Add(new MinimumAgeRequirement(18)));
});
```

**Áp dụng policy:**

```csharp
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]  // Chỉ Admin mới truy cập được
public class UserManagementController : ControllerBase {

  [HttpGet]
  public async Task<IActionResult> GetAllUsers() {
    // Only accessible by Admin
  }
}
```

**Custom Authorization Requirement:**

```csharp
public class MinimumAgeRequirement : IAuthorizationRequirement {
  public int MinimumAge { get; }

  public MinimumAgeRequirement(int minimumAge) {
    MinimumAge = minimumAge;
  }
}

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement> {
  protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    MinimumAgeRequirement requirement
  ) {
    var birthDateClaim = context.User.FindFirst(c => c.Type == "birthdate");

    if (birthDateClaim == null) {
      return Task.CompletedTask;
    }

    var birthDate = DateTime.Parse(birthDateClaim.Value);
    var age = DateTime.Today.Year - birthDate.Year;

    if (age >= requirement.MinimumAge) {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}
```

---

## 4. QUẢN LÝ PHIÊN ĐĂNG NHẬP VÀ REFRESH TOKEN

### 4.1 Tại sao cần Refresh Token?

**Vấn đề:**

- Access token ngắn hạn (5-60 phút) → Tăng bảo mật
- User phải login lại liên tục → Trải nghiệm tồi

**Giải pháp:**

- **Access Token**: Ngắn hạn, dùng cho API requests
- **Refresh Token**: Dài hạn, dùng để lấy access token mới

**Flow:**

```
1. Login → Server trả về { accessToken, refreshToken }
2. Client lưu cả 2 tokens
3. Mỗi request → Gửi accessToken trong header
4. AccessToken hết hạn → Gửi refreshToken để lấy token mới
5. Server validate refreshToken → Trả về token pair mới
```

### 4.2 Refresh Token Implementation

**File: `L3.Infrastructure/Adapters/Security/AuthService.cs`**

```csharp
public async Task<AuthTokens> RefreshAsync(
  string refreshToken,
  CancellationToken ct
) {
  var tokenHandler = new JwtSecurityTokenHandler();
  var key = Encoding.UTF8.GetBytes(jwtOptions.Secret!);

  // 1. Validate refresh token
  try {
    tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = true,
      ValidIssuer = jwtOptions.Issuer,
      ValidateAudience = true,
      ValidAudience = jwtOptions.Audience,
      ClockSkew = TimeSpan.Zero
    }, out var validatedToken);

    var jwtToken = (JwtSecurityToken)validatedToken;

    // 2. Kiểm tra token type
    var tokenType = jwtToken.Claims
      .FirstOrDefault(x => x.Type == "token_type")?.Value;

    if (tokenType != "refresh") {
      throw new AppException("Token không hợp lệ");
    }

    // 3. Kiểm tra blacklist (đã logout chưa?)
    var isBlacklisted = await cache.IsBlacklistedAsync(jwtToken.Id, ct);
    if (isBlacklisted) {
      throw new AppException("Phiên đăng nhập đã bị vô hiệu hóa", 401);
    }

    // 4. Lấy user info
    var userId = jwtToken.Claims
      .First(x => x.Type == ClaimTypes.NameIdentifier).Value;
    var user = await userManager.FindByIdAsync(userId);

    if (user == null || user.IsDeleted) {
      throw new AppException("Tài khoản không hợp lệ", 401);
    }

    // 5. Kiểm tra account lockout
    if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow) {
      throw new AppException("Tài khoản đã bị khóa", 403);
    }

    // 6. Kiểm tra security stamp (password đã đổi chưa?)
    var tokenStamp = jwtToken.Claims
      .FirstOrDefault(x => x.Type == "security_stamp")?.Value;

    if (tokenStamp != user.SecurityStamp) {
      throw new AppException(
        "Thông tin bảo mật đã thay đổi, vui lòng đăng nhập lại",
        401
      );
    }

    // 7. Blacklist refresh token cũ (one-time use)
    var remainingTime = jwtToken.ValidTo - DateTime.UtcNow;
    await cache.BlacklistAsync(jwtToken.Id, remainingTime, ct);

    // 8. Trả về cặp token mới
    var userModel = ToUserModel(user);
    return new AuthTokens(
      jwtService.GenerateAccessToken(userModel),
      jwtService.GenerateRefreshToken(userModel)
    );

  } catch (SecurityTokenException) {
    throw new AppException("Refresh token không hợp lệ hoặc đã hết hạn", 401);
  }
}
```

### 4.3 Token Blacklisting với Redis

**Interface: `L2.Application/Ports/Storage/ICacheStorage.cs`**

```csharp
namespace L2.Application.Ports.Storage;

public interface ICacheStorage {
  Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
  Task SetAsync<T>(string key, T value, TimeSpan? expiration, CancellationToken ct);
  Task RemoveAsync(string key, CancellationToken ct = default);

  // Token blacklisting
  Task BlacklistAsync(string key, TimeSpan duration, CancellationToken ct = default);
  Task<bool> IsBlacklistedAsync(string key, CancellationToken ct = default);
}
```

**Implementation: `L3.Infrastructure/Adapters/Storage/RedisCacheStorage.cs`**

```csharp
using Microsoft.Extensions.Caching.Distributed;

namespace L3.Infrastructure.Adapters.Storage;

public class RedisCacheStorage(IDistributedCache cache) : ICacheStorage {

  public async Task BlacklistAsync(
    string key,
    TimeSpan duration,
    CancellationToken ct = default
  ) {
    var options = new DistributedCacheEntryOptions {
      AbsoluteExpirationRelativeToNow = duration
    };

    // Lưu vào Redis với prefix "blacklist:"
    await cache.SetStringAsync($"blacklist:{key}", "true", options, ct);
  }

  public async Task<bool> IsBlacklistedAsync(
    string key,
    CancellationToken ct = default
  ) {
    var value = await cache.GetStringAsync($"blacklist:{key}", ct);
    return !string.IsNullOrEmpty(value);
  }
}
```

**Cách hoạt động:**

```
1. Khi refresh token → Blacklist token cũ
2. Token ID (claim "jti") → Lưu vào Redis
3. TTL = Thời gian còn lại đến khi token hết hạn
4. Redis tự động xóa key khi hết hạn
5. Mỗi lần validate → Check Redis xem token có bị blacklist không
```

**Cấu hình Redis:**

```csharp
// L3.Infrastructure/Extensions/DistributedExtensions.cs
public static IServiceCollection AddDistributedInfrastructure(
  this IServiceCollection services
) {
  // Redis Cache
  services.AddOptions<RedisCacheOptions>()
    .Configure<IOptions<RedisOptions>>((cacheOptions, redisOptionsRef) => {
      var redisOptions = redisOptionsRef.Value;
      cacheOptions.Configuration = redisOptions.Configuration;
      cacheOptions.InstanceName = redisOptions.InstanceName;
    });

  services.AddStackExchangeRedisCache(_ => {});
  services.AddScoped<ICacheStorage, RedisCacheStorage>();

  return services;
}
```

### 4.4 Logout Mechanism

**Logout từ thiết bị hiện tại:**

```csharp
public async Task LogoutAsync(
  string refreshToken,
  bool revokeAll,
  CancellationToken ct
) {
  if (string.IsNullOrEmpty(refreshToken)) {
    return;
  }

  var tokenHandler = new JwtSecurityTokenHandler();

  try {
    var jwtToken = tokenHandler.ReadJwtToken(refreshToken);

    if (revokeAll) {
      // Logout khỏi TẤT CẢ thiết bị
      var userId = jwtToken.Claims
        .First(x => x.Type == ClaimTypes.NameIdentifier).Value;
      var user = await userManager.FindByIdAsync(userId);

      if (user != null) {
        // Đổi security stamp → Invalidate tất cả tokens
        await userManager.UpdateSecurityStampAsync(user);
      }
    } else {
      // Logout khỏi thiết bị HIỆN TẠI
      var remainingTime = jwtToken.ValidTo - DateTime.UtcNow;

      if (remainingTime.TotalSeconds > 0) {
        await cache.BlacklistAsync(jwtToken.Id, remainingTime, ct);
      }
    }
  } catch {
    // Token không hợp lệ, bỏ qua
  }
}
```

**So sánh 2 cách logout:**

| Feature      | Blacklist Token   | Update Security Stamp              |
| ------------ | ----------------- | ---------------------------------- |
| **Scope**    | Thiết bị hiện tại | Tất cả thiết bị                    |
| **Speed**    | Nhanh             | Nhanh                              |
| **Storage**  | Redis (temporary) | Database (permanent)               |
| **Use case** | Normal logout     | Security incident, password change |

---

## 5. CẤU HÌNH BẢO MẬT VÀ IDENTITY OPTIONS

### 5.1 Password Policy

**Basic Configuration:**

```csharp
services.AddIdentityCore<AppUser>(options => {
  // Password requirements
  options.Password.RequireDigit = true;
  options.Password.RequiredLength = 8;
  options.Password.RequireNonAlphanumeric = true;
  options.Password.RequireUppercase = true;
  options.Password.RequireLowercase = true;
  options.Password.RequiredUniqueChars = 4;
});
```

**Production Best Practices:**

| Setting                | Development | Production |
| ---------------------- | ----------- | ---------- |
| RequiredLength         | 6           | 12+        |
| RequireDigit           | false       | true       |
| RequireNonAlphanumeric | false       | true       |
| RequireUppercase       | false       | true       |
| RequireLowercase       | false       | true       |
| RequiredUniqueChars    | 1           | 6+         |

**Custom Password Validator:**

```csharp
public class CustomPasswordValidator : IPasswordValidator<AppUser> {
  public async Task<IdentityResult> ValidateAsync(
    UserManager<AppUser> manager,
    AppUser user,
    string password
  ) {
    var errors = new List<IdentityError>();

    // Kiểm tra password không chứa username
    if (password.Contains(user.UserName, StringComparison.OrdinalIgnoreCase)) {
      errors.Add(new IdentityError {
        Code = "PasswordContainsUserName",
        Description = "Mật khẩu không được chứa tên người dùng"
      });
    }

    // Kiểm tra password không chứa email
    if (password.Contains(user.Email, StringComparison.OrdinalIgnoreCase)) {
      errors.Add(new IdentityError {
        Code = "PasswordContainsEmail",
        Description = "Mật khẩu không được chứa email"
      });
    }

    // Kiểm tra common passwords
    var commonPasswords = new[] {
      "Password123", "Qwerty123", "Admin123"
    };

    if (commonPasswords.Contains(password, StringComparer.OrdinalIgnoreCase)) {
      errors.Add(new IdentityError {
        Code = "CommonPassword",
        Description = "Mật khẩu này quá phổ biến"
      });
    }

    return errors.Any()
      ? IdentityResult.Failed(errors.ToArray())
      : IdentityResult.Success;
  }
}

// Đăng ký validator
services.AddIdentityCore<AppUser>()
  .AddPasswordValidator<CustomPasswordValidator>();
```

### 5.2 Account Lockout System

**Cấu hình:**

```csharp
services.AddIdentityCore<AppUser>(options => {
  // Lockout settings
  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
  options.Lockout.MaxFailedAccessAttempts = 5;
  options.Lockout.AllowedForNewUsers = true;
});
```

**Tự động lockout khi login sai:**

```csharp
public async Task<AuthTokens> LoginAsync(string email, string password) {
  var user = await userManager.FindByEmailAsync(email);

  if (user == null) {
    throw new AppException("Email hoặc mật khẩu không đúng", 401);
  }

  // Check lockout status
  if (await userManager.IsLockedOutAsync(user)) {
    var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
    var remainingTime = lockoutEnd - DateTimeOffset.UtcNow;

    throw new AppException(
      $"Tài khoản đã bị khóa. Thử lại sau {remainingTime.Value.Minutes} phút",
      403
    );
  }

  // Validate password
  var isPasswordValid = await userManager.CheckPasswordAsync(user, password);

  if (!isPasswordValid) {
    // Tăng failed access count
    await userManager.AccessFailedAsync(user);

    // Check if locked out
    if (await userManager.IsLockedOutAsync(user)) {
      throw new AppException(
        $"Tài khoản đã bị khóa sau {options.Lockout.MaxFailedAccessAttempts} lần thử sai",
        403
      );
    }

    var failedCount = await userManager.GetAccessFailedCountAsync(user);
    var remainingAttempts = options.Lockout.MaxFailedAccessAttempts - failedCount;

    throw new AppException(
      $"Mật khẩu không đúng. Còn {remainingAttempts} lần thử",
      401
    );
  }

  // Reset failed count on successful login
  await userManager.ResetAccessFailedCountAsync(user);

  // Generate tokens...
}
```

**Manual lockout:**

```csharp
// Lock account
public async Task LockUserAsync(Guid userId) {
  var user = await userManager.FindByIdAsync(userId.ToString());

  await userManager.SetLockoutEnabledAsync(user, true);
  await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
}

// Unlock account
public async Task UnlockUserAsync(Guid userId) {
  var user = await userManager.FindByIdAsync(userId.ToString());

  await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
  await userManager.ResetAccessFailedCountAsync(user);
}
```

### 5.3 User Options

```csharp
services.AddIdentityCore<AppUser>(options => {
  // User settings
  options.User.RequireUniqueEmail = true;
  options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
});
```

### 5.4 Token Options (for Password Reset)

```csharp
services.AddIdentityCore<AppUser>(options => {
  // Token settings
  options.Tokens.PasswordResetTokenProvider =
    TokenOptions.DefaultEmailProvider;

  // Optional: Custom token lifespan
  options.Tokens.ProviderMap[TokenOptions.DefaultEmailProvider] =
    new TokenProviderDescriptor(
      typeof(DataProtectorTokenProvider<AppUser>)
    ) {
      TokenLifespan = TimeSpan.FromHours(3)  // 3 hours
    };
});
```

### 5.5 Security Best Practices

**1. Sử dụng Environment Variables:**

```csharp
// appsettings.json
{
  "Jwt": {
    "Secret": "${JWT_SECRET}",  // Biến môi trường
    "Issuer": "YourAPI",
    "Audience": "YourClient"
  }
}
```

**2. Rate Limiting:**

```csharp
// Install: AspNetCoreRateLimit
services.Configure<IpRateLimitOptions>(options => {
  options.GeneralRules = new List<RateLimitRule> {
    new RateLimitRule {
      Endpoint = "*:/api/*/auth/login",
      Period = "1m",
      Limit = 5  // 5 requests per minute
    }
  };
});
```

**3. HTTPS Enforcement:**

```csharp
app.UseHttpsRedirection();
app.UseHsts();  // HTTP Strict Transport Security
```

**4. CORS Configuration:**

```csharp
services.AddCors(options => {
  options.AddPolicy("Production", builder => {
    builder
      .WithOrigins("https://yourdomain.com")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
  });
});
```

**5. Request Logging:**

```csharp
public class RequestLogMiddleware {
  public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
    // Log request details
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    var endpoint = context.Request.Path;
    var method = context.Request.Method;

    _logger.LogInformation(
      "User {UserId} accessed {Method} {Endpoint}",
      userId, method, endpoint
    );

    await next(context);
  }
}
```

---

## 6. KIẾN TRÚC TỔNG THỂ

### 6.1 Layer Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                     L0.API (Presentation Layer)                  │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Controllers/                                                    │
│  ├── Bidder/AuthController.cs                                   │
│  │   ├── POST /api/user/auth/register                          │
│  │   ├── POST /api/user/auth/login                             │
│  │   ├── POST /api/user/auth/refresh                           │
│  │   └── POST /api/user/auth/logout                            │
│  │                                                              │
│  └── Admin/AuthController.cs                                    │
│      └── POST /api/admin/auth/login                            │
│                                                                  │
│  Adapters/Security/                                             │
│  ├── JwtService.cs                                              │
│  │   ├── GenerateAccessToken()                                 │
│  │   └── GenerateRefreshToken()                                │
│  │                                                              │
│  └── CurrentUser.cs                                             │
│      └── Extract user from HttpContext.User                     │
│                                                                  │
│  Extensions/                                                     │
│  └── PresentationExtensions.cs                                  │
│      ├── AddApiAuthentication()                                │
│      └── Configure JWT Bearer                                   │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
                              ↓ MediatR
┌──────────────────────────────────────────────────────────────────┐
│              L2.Application (Business Logic Layer)               │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  UseCases/Auth/Bidder/                                          │
│  ├── Login/                                                      │
│  │   ├── LoginCommand.cs                                        │
│  │   ├── LoginHandler.cs                                        │
│  │   └── LoginValidator.cs                                      │
│  │                                                              │
│  ├── Register/                                                   │
│  ├── RefreshAccess/                                             │
│  ├── Logout/                                                     │
│  ├── ChangePassword/                                            │
│  └── ResetPassword/                                             │
│                                                                  │
│  Ports/Security/ (Interfaces)                                   │
│  ├── IAuthService                                               │
│  ├── IUserService                                               │
│  ├── IJwtService                                                │
│  └── ICurrentUser                                               │
│                                                                  │
│  Ports/Storage/                                                  │
│  └── ICacheStorage                                              │
│                                                                  │
│  Models/                                                         │
│  ├── User.cs                                                     │
│  ├── TokenModel.cs                                              │
│  └── AuthTokens.cs                                              │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
                              ↓ Interface Implementation
┌──────────────────────────────────────────────────────────────────┐
│          L3.Infrastructure (Data Access & External)              │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Identity/                                                       │
│  └── AppUser.cs                                                  │
│      └── Extends IdentityUser<Guid>                            │
│                                                                  │
│  Adapters/Security/                                             │
│  ├── AuthService.cs                                             │
│  │   ├── LoginUserAsync()                                       │
│  │   ├── LoginAdminAsync()                                      │
│  │   ├── RegisterAsync()                                        │
│  │   ├── RefreshAsync()                                         │
│  │   └── LogoutAsync()                                          │
│  │                                                              │
│  └── UserService.cs                                             │
│      ├── GetByIdAsync()                                         │
│      ├── CreateAsync()                                          │
│      ├── UpdateAsync()                                          │
│      ├── LockAsync()                                            │
│      └── UnlockAsync()                                          │
│                                                                  │
│  Adapters/Storage/                                              │
│  └── RedisCacheStorage.cs                                       │
│      ├── BlacklistAsync()                                       │
│      └── IsBlacklistedAsync()                                   │
│                                                                  │
│  Extensions/                                                     │
│  ├── IdentityExtensions.cs                                      │
│  │   └── AddIdentityInfrastructure()                           │
│  │                                                              │
│  └── DistributedExtensions.cs                                   │
│      └── AddDistributedInfrastructure()                        │
│                                                                  │
│  Persistence/                                                    │
│  └── AppDbContext.cs                                            │
│      └── Extends IdentityUserContext<AppUser, Guid>            │
│                                                                  │
│  Options/                                                        │
│  └── JwtOptions.cs                                              │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│                   External Dependencies                          │
├──────────────────────────────────────────────────────────────────┤
│  - PostgreSQL (User storage)                                     │
│  - Redis (Token blacklist)                                       │
│  - SMTP Server (Email for password reset)                        │
└──────────────────────────────────────────────────────────────────┘
```

### 6.2 Dependency Flow

```
Program.cs
    ↓
InfrastructureConfiguration.AddInfrastructure()
    ├── AddConfigurationOptions()
    ├── AddPostgresPersistence()
    ├── AddIdentityInfrastructure()  ← Identity setup
    ├── AddDistributedInfrastructure()  ← Redis
    └── AddMediatorPipeline()

PresentationExtensions.AddPresentationInfrastructure()
    ├── AddRealtime()
    └── AddApiAuthentication()  ← JWT setup
```

---

## 7. FLOWS XÁC THỰC

### 7.1 Flow: Đăng ký tài khoản

```
┌─────────┐                ┌──────────┐                ┌────────────┐
│ Client  │                │   API    │                │   Database │
└────┬────┘                └─────┬────┘                └──────┬─────┘
     │                           │                            │
     │ POST /api/user/auth/register                          │
     │ Body: {                    │                            │
     │   email, password,         │                            │
     │   fullName, phoneNumber    │                            │
     │ }                          │                            │
     ├──────────────────────────>│                            │
     │                           │                            │
     │                           │ RegisterCommand            │
     │                           │    ↓                       │
     │                           │ RegisterHandler            │
     │                           │    ↓                       │
     │                           │ IAuthService.RegisterAsync()│
     │                           │    ↓                       │
     │                           │ Check email exists         │
     │                           ├───────────────────────────>│
     │                           │                            │
     │                           │ User exists?               │
     │                           │<───────────────────────────┤
     │                           │                            │
     │                           │ UserManager.CreateAsync()  │
     │                           │ - Hash password            │
     │                           │ - Generate security stamp  │
     │                           ├───────────────────────────>│
     │                           │                            │
     │                           │ User saved                 │
     │                           │<───────────────────────────┤
     │                           │                            │
     │                           │ IJwtService.GenerateAccessToken()
     │                           │ IJwtService.GenerateRefreshToken()
     │                           │                            │
     │ 200 OK                    │                            │
     │ {                         │                            │
     │   access: {               │                            │
     │     token: "...",         │                            │
     │     expiredAt: "..."      │                            │
     │   },                      │                            │
     │   refresh: {              │                            │
     │     token: "...",         │                            │
     │     expiredAt: "..."      │                            │
     │   }                       │                            │
     │ }                         │                            │
     │<──────────────────────────┤                            │
     │                           │                            │
     │ Store tokens in           │                            │
     │ localStorage/sessionStorage│                           │
     │                           │                            │
```

### 7.2 Flow: Đăng nhập

```
┌─────────┐                ┌──────────┐                ┌────────────┐
│ Client  │                │   API    │                │   Database │
└────┬────┘                └─────┬────┘                └──────┬─────┘
     │                           │                            │
     │ POST /api/user/auth/login  │                            │
     │ Body: {                    │                            │
     │   email, password          │                            │
     │ }                          │                            │
     ├──────────────────────────>│                            │
     │                           │                            │
     │                           │ LoginCommand               │
     │                           │    ↓                       │
     │                           │ LoginHandler               │
     │                           │    ↓                       │
     │                           │ IAuthService.LoginUserAsync()
     │                           │    ↓                       │
     │                           │ UserManager.FindByEmailAsync()
     │                           ├───────────────────────────>│
     │                           │                            │
     │                           │ AppUser                    │
     │                           │<───────────────────────────┤
     │                           │                            │
     │                           │ Check:                     │
     │                           │ - User exists?             │
     │                           │ - Not deleted?             │
     │                           │ - Correct role?            │
     │                           │                            │
     │                           │ UserManager.CheckPasswordAsync()
     │                           │ - Verify password hash     │
     │                           │                            │
     │                           │ UserManager.IsLockedOutAsync()
     │                           │ - Check lockout status     │
     │                           │                            │
     │                           │ UserManager.ResetAccessFailedCountAsync()
     │                           │ - Reset on success         │
     │                           │                            │
     │                           │ Generate tokens            │
     │                           │                            │
     │ 200 OK                    │                            │
     │ { access, refresh }       │                            │
     │<──────────────────────────┤                            │
     │                           │                            │
```

### 7.3 Flow: Gọi API với Access Token

```
┌─────────┐                ┌──────────┐                ┌────────────┐
│ Client  │                │   API    │                │   JWT      │
└────┬────┘                └─────┬────┘                └──────┬─────┘
     │                           │                            │
     │ GET /api/user/profile     │                            │
     │ Header:                   │                            │
     │ Authorization: Bearer {token}                          │
     ├──────────────────────────>│                            │
     │                           │                            │
     │                           │ JWT Middleware             │
     │                           │    ↓                       │
     │                           │ Extract token from header  │
     │                           │    ↓                       │
     │                           │ Validate token             │
     │                           ├───────────────────────────>│
     │                           │ - Verify signature         │
     │                           │ - Check expiration         │
     │                           │ - Validate issuer/audience │
     │                           │<───────────────────────────┤
     │                           │                            │
     │                           │ Extract claims             │
     │                           │ - userId, email, role      │
     │                           │    ↓                       │
     │                           │ Set HttpContext.User       │
     │                           │    ↓                       │
     │                           │ Controller action          │
     │                           │    ↓                       │
     │                           │ ICurrentUser.User          │
     │                           │ - Access userId, role      │
     │                           │    ↓                       │
     │                           │ Business logic             │
     │                           │                            │
     │ 200 OK                    │                            │
     │ { user profile data }     │                            │
     │<──────────────────────────┤                            │
     │                           │                            │
```

### 7.4 Flow: Refresh Access Token

```
┌─────────┐     ┌──────────┐     ┌────────┐     ┌──────────┐
│ Client  │     │   API    │     │  Redis │     │ Database │
└────┬────┘     └─────┬────┘     └───┬────┘     └─────┬────┘
     │                │                │                │
     │ Access token   │                │                │
     │ expired!       │                │                │
     │                │                │                │
     │ POST /api/user/auth/refresh     │                │
     │ Body: {                         │                │
     │   refreshToken                  │                │
     │ }              │                │                │
     ├───────────────>│                │                │
     │                │                │                │
     │                │ Validate refresh token          │
     │                │ - Check signature               │
     │                │ - Check expiration              │
     │                │ - Check token_type = "refresh"  │
     │                │                │                │
     │                │ Check blacklist│                │
     │                ├───────────────>│                │
     │                │ Is blacklisted?│                │
     │                │<───────────────┤                │
     │                │                │                │
     │                │ Extract userId │                │
     │                │ Get user       │                │
     │                ├───────────────────────────────>│
     │                │                │                │
     │                │ AppUser        │                │
     │                │<───────────────────────────────┤
     │                │                │                │
     │                │ Check:         │                │
     │                │ - User exists? │                │
     │                │ - Not deleted? │                │
     │                │ - Not locked?  │                │
     │                │ - Security stamp matches?       │
     │                │                │                │
     │                │ Blacklist old refresh token     │
     │                ├───────────────>│                │
     │                │ Set TTL        │                │
     │                │<───────────────┤                │
     │                │                │                │
     │                │ Generate new token pair         │
     │                │                │                │
     │ 200 OK         │                │                │
     │ {              │                │                │
     │   access: {...},                │                │
     │   refresh: {...}                │                │
     │ }              │                │                │
     │<───────────────┤                │                │
     │                │                │                │
     │ Update stored  │                │                │
     │ tokens         │                │                │
     │                │                │                │
```

### 7.5 Flow: Logout

```
┌─────────┐     ┌──────────┐     ┌────────┐     ┌──────────┐
│ Client  │     │   API    │     │  Redis │     │ Database │
└────┬────┘     └─────┬────┘     └───┬────┘     └─────┬────┘
     │                │                │                │
     │ POST /api/user/auth/logout      │                │
     │ Body: {                         │                │
     │   refreshToken,                 │                │
     │   revokeAll: false              │                │
     │ }              │                │                │
     ├───────────────>│                │                │
     │                │                │                │
     │                │ Parse refresh token             │
     │                │ Extract: jti, userId            │
     │                │                │                │
     │                │ if (revokeAll) {                │
     │                │   // Logout all devices         │
     │                │   Get user     │                │
     │                ├───────────────────────────────>│
     │                │                │                │
     │                │   UpdateSecurityStampAsync()    │
     │                │   // Invalidate ALL tokens      │
     │                ├───────────────────────────────>│
     │                │<───────────────────────────────┤
     │                │                │                │
     │                │ } else {       │                │
     │                │   // Logout current device      │
     │                │   Blacklist token               │
     │                ├───────────────>│                │
     │                │ TTL = remaining time            │
     │                │<───────────────┤                │
     │                │ }              │                │
     │                │                │                │
     │ 200 OK         │                │                │
     │ "Đăng xuất thành công"          │                │
     │<───────────────┤                │                │
     │                │                │                │
     │ Clear tokens   │                │                │
     │ from storage   │                │                │
     │                │                │                │
```

### 7.6 Flow: Đổi mật khẩu

```
┌─────────┐                ┌──────────┐                ┌────────────┐
│ Client  │                │   API    │                │   Database │
└────┬────┘                └─────┬────┘                └──────┬─────┘
     │                           │                            │
     │ POST /api/user/auth/change-password                    │
     │ Header: Authorization: Bearer {token}                  │
     │ Body: {                    │                            │
     │   oldPassword,             │                            │
     │   newPassword              │                            │
     │ }                          │                            │
     ├──────────────────────────>│                            │
     │                           │                            │
     │                           │ Extract userId from JWT    │
     │                           │    ↓                       │
     │                           │ Get user                   │
     │                           ├───────────────────────────>│
     │                           │                            │
     │                           │ AppUser                    │
     │                           │<───────────────────────────┤
     │                           │                            │
     │                           │ UserManager.ChangePasswordAsync()
     │                           │ - Verify old password      │
     │                           │ - Hash new password        │
     │                           │ - Update SecurityStamp     │
     │                           ├───────────────────────────>│
     │                           │                            │
     │                           │ Password updated           │
     │                           │<───────────────────────────┤
     │                           │                            │
     │                           │ Generate new token pair    │
     │                           │ (vì security stamp changed)│
     │                           │                            │
     │ 200 OK                    │                            │
     │ { access, refresh }       │                            │
     │<──────────────────────────┤                            │
     │                           │                            │
     │ Note: Old tokens on other │                            │
     │ devices are now invalid   │                            │
     │ (security stamp mismatch) │                            │
     │                           │                            │
```

### 7.7 Flow: Quên mật khẩu & Reset

```
┌─────────┐     ┌──────────┐     ┌──────────┐     ┌────────┐
│ Client  │     │   API    │     │   Email  │     │   DB   │
└────┬────┘     └─────┬────┘     └─────┬────┘     └───┬────┘
     │                │                │                │
     │ 1. Request Password Reset       │                │
     │ POST /api/user/auth/forgot-password             │
     │ Body: { email }                 │                │
     ├───────────────>│                │                │
     │                │                │                │
     │                │ Get user by email               │
     │                ├───────────────────────────────>│
     │                │                │                │
     │                │ AppUser        │                │
     │                │<───────────────────────────────┤
     │                │                │                │
     │                │ UserManager.GeneratePasswordResetTokenAsync()
     │                │ - Create token with DataProtector
     │                │ - Token lifetime: 3 hours      │
     │                │                │                │
     │                │ Base64Url encode token          │
     │                │                │                │
     │                │ Send email     │                │
     │                ├───────────────>│                │
     │                │ Subject: "Reset Password"       │
     │                │ Link: https://app.com/reset?token={token}&email={email}
     │                │                │                │
     │ 200 OK         │                │                │
     │ "Email đã được gửi"             │                │
     │<───────────────┤                │                │
     │                │                │                │
     │                │                │ ✉ Email sent  │
     │                │                │                │
     ├────────────────────────────────────────────────>│
     │ User clicks link in email       │                │
     │                │                │                │
     │ 2. Reset Password               │                │
     │ POST /api/user/auth/reset-password              │
     │ Body: {                         │                │
     │   email,                        │                │
     │   token,                        │                │
     │   newPassword                   │                │
     │ }              │                │                │
     ├───────────────>│                │                │
     │                │                │                │
     │                │ Get user       │                │
     │                ├───────────────────────────────>│
     │                │                │                │
     │                │ Base64Url decode token          │
     │                │                │                │
     │                │ UserManager.ResetPasswordAsync()│
     │                │ - Validate token (expiration)   │
     │                │ - Update password hash          │
     │                ├───────────────────────────────>│
     │                │                │                │
     │                │ UserManager.UpdateSecurityStampAsync()
     │                │ - Invalidate all existing tokens│
     │                ├───────────────────────────────>│
     │                │                │                │
     │ 200 OK         │                │                │
     │ "Mật khẩu đã được đặt lại"      │                │
     │<───────────────┤                │                │
     │                │                │                │
```

---

## 8. DEMO PROJECT STRUCTURE

### 8.1 Cấu trúc Project đơn giản

Để demo Chương 6, bạn có thể tạo project đơn giản với cấu trúc sau:

```
IdentityJwtDemo/
├── IdentityJwtDemo.API/          # Presentation Layer
│   ├── Controllers/
│   │   └── AuthController.cs
│   ├── Extensions/
│   │   └── JwtExtensions.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── IdentityJwtDemo.Application/  # Business Logic
│   ├── Commands/
│   │   ├── LoginCommand.cs
│   │   └── RegisterCommand.cs
│   ├── DTOs/
│   │   └── AuthResponse.cs
│   └── Services/
│       └── IAuthService.cs
│
└── IdentityJwtDemo.Infrastructure/  # Data Access
    ├── Data/
    │   └── AppDbContext.cs
    ├── Identity/
    │   └── AppUser.cs
    └── Services/
        ├── AuthService.cs
        └── JwtService.cs
```

### 8.2 Minimal Working Example

**Step 1: Tạo project**

```bash
dotnet new webapi -n IdentityJwtDemo.API
cd IdentityJwtDemo.API

# Install packages
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package System.IdentityModel.Tokens.Jwt
```

**Step 2: AppUser.cs**

```csharp
public class AppUser : IdentityUser<Guid> {
  public string FullName { get; set; } = null!;
}
```

**Step 3: AppDbContext.cs**

```csharp
public class AppDbContext : IdentityUserContext<AppUser, Guid> {
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
```

**Step 4: JwtService.cs**

```csharp
public class JwtService {
  private readonly string _secret = "your-256-bit-secret-key-here-must-be-32-chars";

  public string GenerateToken(AppUser user) {
    var claims = new[] {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email!),
      new Claim(ClaimTypes.Name, user.FullName)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: "DemoAPI",
      audience: "DemoClient",
      claims: claims,
      expires: DateTime.UtcNow.AddHours(1),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
```

**Step 5: AuthController.cs**

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
  private readonly UserManager<AppUser> _userManager;
  private readonly JwtService _jwtService;

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequest request) {
    var user = new AppUser {
      UserName = request.Email,
      Email = request.Email,
      FullName = request.FullName
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded) {
      return BadRequest(result.Errors);
    }

    var token = _jwtService.GenerateToken(user);
    return Ok(new { token });
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request) {
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password)) {
      return Unauthorized("Invalid credentials");
    }

    var token = _jwtService.GenerateToken(user);
    return Ok(new { token });
  }

  [Authorize]
  [HttpGet("profile")]
  public IActionResult GetProfile() {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = User.FindFirstValue(ClaimTypes.Email);

    return Ok(new { userId, email });
  }
}
```

**Step 6: Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer("ConnectionString"));

// Identity
builder.Services.AddIdentityCore<AppUser>()
  .AddEntityFrameworkStores<AppDbContext>();

// JWT Authentication
var key = Encoding.UTF8.GetBytes("your-256-bit-secret-key-here-must-be-32-chars");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = true,
      ValidIssuer = "DemoAPI",
      ValidateAudience = true,
      ValidAudience = "DemoClient",
      ValidateLifetime = true
    };
  });

builder.Services.AddScoped<JwtService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 9. KẾT LUẬN

### 9.1 Checklist Implementation

- ✅ **Identity System**
  - [ ] Cài đặt ASP.NET Core Identity
  - [ ] Tạo custom AppUser entity
  - [ ] Cấu hình DbContext
  - [ ] Thiết lập Password Policy

- ✅ **JWT Authentication**
  - [ ] Cấu hình JWT options
  - [ ] Implement JWT generation
  - [ ] Configure JWT Bearer middleware
  - [ ] Implement CurrentUser service

- ✅ **Authorization**
  - [ ] Define roles/claims
  - [ ] Implement role checking
  - [ ] Setup authorization policies
  - [ ] Apply [Authorize] attributes

- ✅ **Refresh Token**
  - [ ] Implement token refresh logic
  - [ ] Setup Redis for blacklisting
  - [ ] Handle token expiration
  - [ ] Implement logout mechanism

- ✅ **Security**
  - [ ] Strong password policy
  - [ ] Account lockout
  - [ ] Security stamp validation
  - [ ] HTTPS enforcement
  - [ ] Rate limiting
  - [ ] Request logging

### 9.2 Best Practices Summary

1. **Secrets Management**
   - Không hardcode secrets
   - Sử dụng Environment Variables hoặc Azure Key Vault
   - Rotate secrets định kỳ

2. **Token Lifetime**
   - Access token: 5-60 phút
   - Refresh token: 1-7 ngày
   - Password reset token: 1-3 giờ

3. **Password Policy**
   - Minimum 12 characters
   - Require uppercase, lowercase, digit, special char
   - Validate against common passwords
   - Don't allow username/email in password

4. **Error Handling**
   - Không expose chi tiết lỗi ra ngoài
   - Log chi tiết internally
   - Trả về generic error messages

5. **Database**
   - Index trên Email, UserName
   - Soft delete thay vì hard delete
   - Audit trail cho sensitive operations

6. **Monitoring**
   - Log all authentication attempts
   - Monitor failed login attempts
   - Alert on suspicious activities
   - Track token refresh patterns

### 9.3 Common Pitfalls

❌ **Đừng:**

- Lưu password plaintext
- Lưu JWT secret trong code
- Sử dụng weak secret key (< 256 bits)
- Forget to validate token expiration
- Allow long-lived access tokens
- Skip HTTPS in production
- Expose detailed error messages
- Forget to blacklist tokens on logout

✅ **Nên:**

- Hash passwords với bcrypt/PBKDF2
- Store secrets securely
- Use strong, random secret keys
- Validate all token properties
- Use short-lived access tokens
- Enforce HTTPS everywhere
- Return generic error messages
- Implement proper logout

### 9.4 Resources

**Documentation:**

- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT.io](https://jwt.io/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

**NuGet Packages:**

- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.IdentityModel.Tokens.Jwt
- StackExchange.Redis (for token blacklisting)

**Tools:**

- [JWT Debugger](https://jwt.io/#debugger)
- [Postman](https://www.postman.com/) for API testing
- [Redis Commander](https://github.com/joeferner/redis-commander) for Redis debugging

---

## PHỤ LỤC

### A. Sample API Requests

**Register:**

```bash
curl -X POST https://localhost:5001/api/user/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!",
    "fullName": "John Doe"
  }'
```

**Login:**

```bash
curl -X POST https://localhost:5001/api/user/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'
```

**Access Protected Endpoint:**

```bash
curl -X GET https://localhost:5001/api/user/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Refresh Token:**

```bash
curl -X POST https://localhost:5001/api/user/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }'
```

**Logout:**

```bash
curl -X POST https://localhost:5001/api/user/auth/logout \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "revokeAll": false
  }'
```

### B. Database Schema

**AspNetUsers Table:**

```sql
CREATE TABLE AspNetUsers (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  UserName NVARCHAR(256) NOT NULL,
  NormalizedUserName NVARCHAR(256),
  Email NVARCHAR(256),
  NormalizedEmail NVARCHAR(256),
  EmailConfirmed BIT NOT NULL,
  PasswordHash NVARCHAR(MAX),
  SecurityStamp NVARCHAR(MAX),
  PhoneNumber NVARCHAR(50),
  LockoutEnd DATETIMEOFFSET,
  LockoutEnabled BIT NOT NULL,
  AccessFailedCount INT NOT NULL,

  -- Custom fields
  FullName NVARCHAR(100) NOT NULL,
  Url NVARCHAR(255),
  Role NVARCHAR(50) NOT NULL,
  CreatedAt DATETIME2 NOT NULL,
  DeletedAt DATETIME2,
  IsDeleted BIT NOT NULL
);

CREATE INDEX IX_AspNetUsers_NormalizedEmail ON AspNetUsers(NormalizedEmail);
CREATE INDEX IX_AspNetUsers_NormalizedUserName ON AspNetUsers(NormalizedUserName);
```

### C. Environment Variables Template

```bash
# .env file
JWT_SECRET=your-super-secret-key-must-be-at-least-32-characters-long
JWT_ISSUER=YourAPI
JWT_AUDIENCE=YourClient
JWT_ACCESS_EXPIRATION=60
JWT_REFRESH_EXPIRATION=1440

DATABASE_CONNECTION=Server=localhost;Database=YourDB;User=sa;Password=xxx
REDIS_CONNECTION=localhost:6379
```

---

**Tài liệu này được tạo để hỗ trợ việc hiểu và triển khai hệ thống Authentication & Authorization với JWT và ASP.NET Core Identity theo best practices.**

**Version**: 1.0  
**Last Updated**: February 2026  
**Author**: Technical Documentation Team
