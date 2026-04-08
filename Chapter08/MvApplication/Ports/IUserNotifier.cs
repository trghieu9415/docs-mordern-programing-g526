namespace MvApplication.Ports;

public interface IUserNotifier {
  Task SendPersonalMessageAsync(string userId, string message, CancellationToken ct = default);
}
