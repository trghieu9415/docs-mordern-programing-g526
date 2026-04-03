using L2.Application.Exceptions;
using MvApplication.Ports;
using MvApplication.Repositories;
using MvDomain.Entities;

namespace MvInfrastructure.Adapters;

public class AuthService(
  IRepository<User> userRepository,
  IJwtService jwtService
) : IAuthService {
  public async Task<string> LoginAsync(string username, string password, CancellationToken ct) {
    var user = await userRepository.GetFirstAsync(x => x.Username == username && x.PlainPassword == password, ct);

    if (user == null || user.PlainPassword != password) {
      throw new WorkflowException("Sai tài khoản hoặc mật khẩu!");
    }

    return jwtService.GenerateToken(user);
  }

  public async Task<string> RegisterAsync(string username, string email, string password, CancellationToken ct) {
    if (await userRepository.GetFirstAsync(x => x.Username == username, ct) != null) {
      throw new WorkflowException("Tên đăng nhập này có người sử dụng.");
    }

    var newUser = User.Create(username, email, password);
    await userRepository.CreateAsync(newUser, ct);
    return jwtService.GenerateToken(newUser);
  }
}
