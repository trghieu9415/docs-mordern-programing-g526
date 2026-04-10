using MediatR;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.Auth.Logout;

public class LogoutHandler(IAuthService authService) : IRequestHandler<LogoutCommand>
{

    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            await authService.LogoutAsync(request.RefreshToken, ct);
        }
    }
}
