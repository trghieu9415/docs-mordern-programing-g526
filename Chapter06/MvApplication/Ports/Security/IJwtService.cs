using MvApplication.Models;

namespace MvApplication.Ports.Security;

public interface IJwtService
{
    TokenModel GenerateAccessToken(User user);
    TokenModel GenerateRefreshToken(User user);
}
