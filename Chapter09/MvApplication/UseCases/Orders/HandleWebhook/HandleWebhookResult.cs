namespace MvApplication.UseCases.Orders.HandleWebhook;

public record HandleWebhookResult(
  bool Processed,
  bool IsSuccess,
  string Message,
  Guid? OrderId
);
