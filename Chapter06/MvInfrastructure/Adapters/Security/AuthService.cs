using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MvApplication.Exceptions;
using MvApplication.Models;
using MvApplication.Ports.Security;
using MvInfrastructure.Identity;
using MvInfrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MvInfrastructure.Adapters.Security;

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

        // Tạo user mới
        var appUser = new AppUser
        {
            UserName = userName,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(appUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppException($"Không thể tạo tài khoản: {errors}");
        }

        // Generate tokens
        var user = appUser.ToUser();
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);

        return new AuthTokens(accessToken, refreshToken);
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

        // Kiểm tra account lockout
        if (await userManager.IsLockedOutAsync(appUser))
        {
            throw new AppException("Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần");
        }

        // Kiểm tra password
        var isValidPassword = await userManager.CheckPasswordAsync(appUser, password);
        if (!isValidPassword)
        {
            // Tăng failed attempts
            await userManager.AccessFailedAsync(appUser);
            throw new AppException("Email hoặc mật khẩu không đúng");
        }

        // Reset failed attempts
        await userManager.ResetAccessFailedCountAsync(appUser);

        // Cập nhật last login
        appUser.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(appUser);

        // Generate tokens
        var user = appUser.ToUser();
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);

        return new AuthTokens(accessToken, refreshToken);
    }

    public async Task<AuthTokens> RefreshAsync(
      string refreshToken,
      CancellationToken ct = default
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

        try
        {
            // Validate refresh token
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

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new AppException("Token không hợp lệ");
            }

            // Lấy user từ token
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                throw new AppException("Token không hợp lệ");
            }

            var appUser = await userManager.FindByIdAsync(userId.ToString());
            if (appUser == null)
            {
                throw new AppException("User không tồn tại");
            }

            // Kiểm tra SecurityStamp
            var tokenSecurityStamp = principal.FindFirstValue("SecurityStamp");
            if (tokenSecurityStamp != appUser.SecurityStamp)
            {
                throw new AppException("Token đã bị vô hiệu hóa");
            }

            // Generate new tokens
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

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        // Có thể implement token blacklisting ở đây nếu cần
        // Hoặc update SecurityStamp để invalidate tất cả tokens
        await Task.CompletedTask;
    }
}
