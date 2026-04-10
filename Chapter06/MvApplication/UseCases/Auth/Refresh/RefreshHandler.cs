using MediatR;
using MvApplication.Models;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.Auth.Refresh;

public class RefreshHandler(IAuthService authService) : IRequestHandler<RefreshCommand, AuthTokens>
{

    public async Task<AuthTokens> Handle(RefreshCommand request, CancellationToken ct)
    {
        return await authService.RefreshAsync(request.RefreshToken, ct);
    }
}
