using MediatR;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.Auth.RevokeTokens;

public class RevokeTokensHandler(
  IAuthService authService,
  ICurrentUser currentUser
) : IRequestHandler<RevokeTokensCommand>
{
    public async Task Handle(RevokeTokensCommand request, CancellationToken ct)
    {
        await authService.RevokeTokensAsync(currentUser.User.Id, ct);
    }
}
