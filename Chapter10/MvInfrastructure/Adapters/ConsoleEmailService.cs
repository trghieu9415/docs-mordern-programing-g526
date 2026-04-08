using MvApplication.Ports;

namespace MvInfrastructure.Adapters;

public class ConsoleEmailService : IEmailService {
  public async Task SendOrderConfirmationEmailAsync(string email, string orderId, decimal totalPrice,
    CancellationToken ct = default) {
    await Task.Delay(1000, ct);

    Console.WriteLine("--------------------------------------------------");
    Console.WriteLine($"[EMAIL SENT] To: {email}");
    Console.WriteLine($"[SUBJECT] Xác nhận đơn hàng #{orderId}");
    Console.WriteLine($"[CONTENT] Cảm ơn bro đã mua hàng. Tổng thanh toán: {totalPrice:N0} VNĐ");
    Console.WriteLine("--------------------------------------------------");
  }
}
