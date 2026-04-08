namespace MvApplication.Ports;

public interface ICurrentUser {
  Guid Id { get; }
  string Username { get; }
  string Email { get; }
}
