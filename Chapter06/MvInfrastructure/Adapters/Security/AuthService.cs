using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MvApplication.Exceptions;
using MvApplication.Models;
using MvApplication.Ports.Security;
using MvInfrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MvInfrastructure.Identity;

namespace MvInfrastructure.Adapters.Security;

public class AuthService(
  UserManager<ApplicationUser> userManager,
  IJwtService jwtService,
  JwtOptions jwtOptions
) : IAuthService
{
    public async Task<AuthTokens> RegisterAsync(
      string userName,
      string email,
      string password,
      UserRole role,
      CancellationToken ct = default
    )
    {
        // Kiểm tra email đã tồn tại
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new AppException("Email đã được sử dụng");
        }

        var applicationUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            LockoutEnabled = true
        };

        var result = await userManager.CreateAsync(applicationUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppException($"Không thể tạo tài khoản: {errors}");
        }

        return GenerateTokens(applicationUser);
    }

    public async Task<AuthTokens> LoginAsync(
      string email,
      string password,
      CancellationToken ct = default
    )
    {
        var appUser = await userManager.FindByEmailAsync(email);
        if (appUser == null)
        {
            throw new AppException("Email hoặc mật khẩu không đúng");
        }

        if (await userManager.IsLockedOutAsync(appUser))
        {
            throw BuildLockoutException(await userManager.GetLockoutEndDateAsync(appUser));
        }

        var isValidPassword = await userManager.CheckPasswordAsync(appUser, password);
        if (!isValidPassword)
        {
            await userManager.AccessFailedAsync(appUser);

            if (await userManager.IsLockedOutAsync(appUser))
            {
                throw BuildLockoutException(await userManager.GetLockoutEndDateAsync(appUser));
            }

            throw new AppException("Email hoặc mật khẩu không đúng");
        }

        await userManager.ResetAccessFailedCountAsync(appUser);

        appUser.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(appUser);

        return GenerateTokens(appUser);
    }

    public async Task<AuthTokens> RefreshAsync(
      string refreshToken,
      CancellationToken ct = default
    )
    {
        try
        {
            var principal = ValidateJwtToken(refreshToken, validateLifetime: true);
            var userId = GetUserId(principal);
            var tokenSecurityStamp = GetSecurityStamp(principal);
            var appUser = await userManager.FindByIdAsync(userId.ToString());
            if (appUser == null)
            {
                throw new AppException("User không tồn tại");
            }

            if (tokenSecurityStamp != appUser.SecurityStamp)
            {
                throw new AppException("Refresh token đã bị vô hiệu hóa", 401);
            }

            return GenerateTokens(appUser);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException("Token không hợp lệ hoặc đã hết hạn");
        }
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        try
        {
            var principal = ValidateJwtToken(refreshToken, validateLifetime: false);
            var userId = GetUserId(principal);
            await RevokeTokensAsync(userId, ct);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException("Không thể đăng xuất do refresh token không hợp lệ");
        }
    }

    public async Task RevokeTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());
        if (applicationUser == null)
        {
            throw new AppException("User không tồn tại", 404);
        }

        var result = await userManager.UpdateSecurityStampAsync(applicationUser);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppException($"Không thể thu hồi token: {errors}");
        }
    }

    private AuthTokens GenerateTokens(ApplicationUser applicationUser)
    {
        var user = applicationUser.ToUser();
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);

        return new AuthTokens(accessToken, refreshToken);
    }

    private ClaimsPrincipal ValidateJwtToken(string token, bool validateLifetime)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new AppException("Token không hợp lệ");
        }

        return principal;
    }

    private static Guid GetUserId(ClaimsPrincipal principal)
    {
        var userIdText = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var userId))
        {
            throw new AppException("Token không chứa UserId hợp lệ");
        }

        return userId;
    }

    private static string GetSecurityStamp(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(AuthClaimTypes.SecurityStamp)
          ?? throw new AppException("Token không chứa SecurityStamp");
    }

    private static AppException BuildLockoutException(DateTimeOffset? lockoutEnd)
    {
        var lockedUntil = lockoutEnd?.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        var message = lockedUntil is null
          ? "Tài khoản đã bị khóa trong 15 phút do đăng nhập sai quá 5 lần."
          : $"Tài khoản đã bị khóa đến {lockedUntil} do đăng nhập sai quá 5 lần.";

        return new AppException(message, 423);
    }
}
