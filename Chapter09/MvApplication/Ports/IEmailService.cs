namespace MvApplication.Ports;

public interface IEmailService {
  Task SendTicketIssuedAsync(string toEmail, string eventName, string ticketCode, Guid orderId, int quantity, CancellationToken ct = default);
}
