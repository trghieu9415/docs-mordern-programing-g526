using MediatR;
using MvApplication.Abstractions;
using MvApplication.Ports;

namespace MvApplication.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
  IUnitOfWork unitOfWork,
  IEventDispatcher eventDispatcher
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : ITransactional {
  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken ct
  ) {
    await unitOfWork.BeginTransactionAsync(ct);

    try {
      var response = await next();

      await eventDispatcher.DispatchEventsAsync(ct);
      await unitOfWork.SaveChangesAsync(ct);
      await unitOfWork.CommitTransactionAsync(ct);
      return response;
    } catch (Exception) {
      await unitOfWork.RollbackTransactionAsync(ct);
      throw;
    }
  }
}
