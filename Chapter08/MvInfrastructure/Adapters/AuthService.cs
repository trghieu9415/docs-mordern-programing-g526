using MvApplication.Exceptions;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvInfrastructure.Adapters;

public class AuthService(
  IUserDataStore userStore,
  IJwtService jwtService
) : IAuthService {
  public async Task<string> LoginAsync(string username, string password, CancellationToken ct) {
    var user = await userStore.GetByUsernameAsync(username, ct);

    if (user == null || user.PlainPassword != password) {
      throw new WorkflowException("Sai tài khoản hoặc mật khẩu!");
    }

    return jwtService.GenerateToken(user);
  }

  public async Task<string> RegisterAsync(string username, string password, CancellationToken ct) {
    if (await userStore.ExistsAsync(username, ct)) {
      throw new WorkflowException("Tên đăng nhập này có người sử dụng.");
    }

    var newUser = User.Create(username, password);
    await userStore.AddUserAsync(newUser, ct);
    return jwtService.GenerateToken(newUser);
  }
}
