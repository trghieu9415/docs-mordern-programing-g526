using MvApplication.Models;

namespace MvApplication.Ports.Security;

public interface IAuthService
{
    Task<AuthTokens> RegisterAsync(string userName, string email, string password, UserRole role, CancellationToken ct = default);
    Task<AuthTokens> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<AuthTokens> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}
