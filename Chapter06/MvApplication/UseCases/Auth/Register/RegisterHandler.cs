using MediatR;
using MvApplication.Models;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.Auth.Register;

public class RegisterHandler(IAuthService authService) : IRequestHandler<RegisterCommand, AuthTokens>
{

    public async Task<AuthTokens> Handle(RegisterCommand request, CancellationToken ct)
    {
        return await authService.RegisterAsync(
          request.UserName,
          request.Email,
          request.Password,
          request.Role,
          ct
        );
    }
}
