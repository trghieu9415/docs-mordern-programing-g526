using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IUserDataStore {
  Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
  Task AddUserAsync(User user, CancellationToken ct);
  Task<bool> ExistsAsync(string username, CancellationToken ct);
}
