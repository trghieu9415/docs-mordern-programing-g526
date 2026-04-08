using MassTransit;
using MediatR;
using MvApplication.Ports;
using MvApplication.UseCases.UpdateUserPoint;
using MvDomain.Events;

namespace MvWorker.Consumers;

public class OrderCreatedConsumer(
  IBackgroundTaskQueue taskQueue
) : IConsumer<OrderCreatedEvent> {
  public async Task Consume(ConsumeContext<OrderCreatedEvent> context) {
    // Console.WriteLine($"Retry Lần thứ {context.GetRetryAttempt()} - Thời gian: {DateTime.Now}");
    // throw new Exception($"Test exception - At: {DateTime.Now} - Order: {context.Message.OrderId}");
    var msg = context.Message;

    await taskQueue.QueueAsync<IEmailService>((eS, ctx) =>
      eS.SendOrderConfirmationEmailAsync(msg.CustomerEmail, msg.OrderId.ToString(), msg.TotalAmount, ctx)
    );

    await taskQueue.QueueAsync<IMediator>((mediator, ctx) =>
      mediator.Send(new UpdateUserPointCommand(msg.UserId, 10), ctx)
    );
  }
}
