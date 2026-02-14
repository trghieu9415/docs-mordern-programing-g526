using System.Security.Claims;
using MvApplication.Models;
using MvApplication.Ports.Security;

namespace MvPresentation.Adapters.Security;

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
