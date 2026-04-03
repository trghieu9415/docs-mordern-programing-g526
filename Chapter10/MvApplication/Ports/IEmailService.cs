namespace MvApplication.Ports;

public interface IEmailService {
  Task SendOrderConfirmationEmailAsync(string email, string orderId, decimal totalPrice,
    CancellationToken ct = default);
}
