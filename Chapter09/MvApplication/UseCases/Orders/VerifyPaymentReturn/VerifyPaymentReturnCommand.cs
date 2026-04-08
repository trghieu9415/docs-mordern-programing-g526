using MediatR;

namespace MvApplication.UseCases.Orders.VerifyPaymentReturn;

public record VerifyPaymentReturnCommand(
  Guid OrderId,
  Dictionary<string, string> CallbackData
) : IRequest<VerifyPaymentReturnResult>;
