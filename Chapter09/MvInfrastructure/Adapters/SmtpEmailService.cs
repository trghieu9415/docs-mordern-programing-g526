using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MvApplication.Ports;
using MvInfrastructure.Configuration;
using MvInfrastructure.Exceptions;

namespace MvInfrastructure.Adapters;

public class SmtpEmailService(IOptions<EmailOptions> options) : IEmailService {
  private readonly EmailOptions _options = options.Value;

  public async Task SendTicketIssuedAsync(
    string toEmail,
    string eventName,
    string ticketCode,
    Guid orderId,
    int quantity,
    CancellationToken ct = default
  ) {
    if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password)) {
      throw new InfrastructureException("Email chua duoc cau hinh day du.");
    }

    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
    message.To.Add(MailboxAddress.Parse(toEmail));
    message.Subject = $"Ve dien tu cho su kien {eventName}";
    message.Body = new BodyBuilder {
      HtmlBody = $"""
                  <h2>Xac nhan dat ve thanh cong</h2>
                  <p>Su kien: <strong>{eventName}</strong></p>
                  <p>Ma don: <strong>{orderId}</strong></p>
                  <p>So luong ve: <strong>{quantity}</strong></p>
                  <p>Ma ve dien tu: <strong>{ticketCode}</strong></p>
                  <p>Ban co the dung ma ve nay de doi chieu khi check-in.</p>
                  """
    }.ToMessageBody();

    using var client = new SmtpClient();
    var secureSocketOption = ResolveSocketOption(_options.Port, _options.EnableSsl);

    try {
      await client.ConnectAsync(_options.Host, _options.Port, secureSocketOption, ct);
      await client.AuthenticateAsync(_options.Username, _options.Password, ct);
      await client.SendAsync(message, ct);
      await client.DisconnectAsync(true, ct);
    } catch (Exception ex) {
      throw new InfrastructureException($"Gui email that bai: {ex.Message}");
    }
  }

  private static SecureSocketOptions ResolveSocketOption(int port, bool enableSsl) {
    if (!enableSsl) {
      return SecureSocketOptions.None;
    }

    return port switch {
      465 => SecureSocketOptions.SslOnConnect,
      587 => SecureSocketOptions.StartTls,
      _ => SecureSocketOptions.Auto
    };
  }
}
