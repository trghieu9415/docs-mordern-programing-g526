using MvApplication.Ports;
using MvDomain.Entities;

namespace MvInfrastructure.Adapters;

public class UserDataStore : IUserDataStore {
  private readonly List<User> _users = [];

  public UserDataStore() {
    _users.Add(User.Create("123123", "123123"));
    _users.Add(User.Create("zxczxc", "zxczxc"));
  }

  public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct) {
    await Task.Delay(500, ct);
    return _users.FirstOrDefault(u => u.Username == username);
  }

  public async Task AddUserAsync(User user, CancellationToken ct) {
    _users.Add(user);
    await Task.CompletedTask;
  }

  public async Task<bool> ExistsAsync(string username, CancellationToken ct) {
    return await Task.FromResult(_users.Any(u => u.Username == username));
  }
}
