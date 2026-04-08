using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.Login;

public class LoginHandler(
  IAuthService authService
) : IRequestHandler<LoginCommand, LoginResult> {
  public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct) {
    var token = await authService.LoginAsync(request.Username, request.Password, ct);
    return new LoginResult(token);
  }
}
