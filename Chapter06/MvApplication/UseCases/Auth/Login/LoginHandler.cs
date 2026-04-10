using MediatR;
using MvApplication.Models;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.Auth.Login;

public class LoginHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthTokens>
{

    public async Task<AuthTokens> Handle(LoginCommand request, CancellationToken ct)
    {
        return await authService.LoginAsync(
          request.Email,
          request.Password,
          ct
        );
    }
}
