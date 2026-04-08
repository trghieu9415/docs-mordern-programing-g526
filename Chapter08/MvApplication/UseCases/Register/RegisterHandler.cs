using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.Register;

public class RegisterHandler(
  IAuthService authService
) : IRequestHandler<RegisterCommand, RegisterResult> {
  public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct) {
    var token = await authService.RegisterAsync(request.Username, request.Password, ct);
    return new RegisterResult(token);
  }
}
