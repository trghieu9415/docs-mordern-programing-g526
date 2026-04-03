namespace MvApplication.Ports;

public interface IAuthService {
  Task<string> LoginAsync(string username, string password, CancellationToken ct);
  Task<string> RegisterAsync(string username, string email, string password, CancellationToken ct);
}
