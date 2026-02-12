using FluentValidation;
using MediatR;
using ValidationException = MvApplication.Exceptions.ValidationException;

namespace MvApplication.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse> {
  public async Task<TResponse> Handle(
    TRequest request, RequestHandlerDelegate<TResponse> next,
    CancellationToken ct
  ) {
    if (!validators.Any()) {
      return await next();
    }

    var context = new ValidationContext<TRequest>(request);

    var validationResults = await Task.WhenAll(
      validators.Select(v => v.ValidateAsync(context, ct))
    );

    var failures = validationResults
      .SelectMany(r => r.Errors)
      .Where(f => f != null)
      .ToList();

    if (failures.Count == 0) {
      return await next();
    }

    var errorMessages = failures
      .Select(x => x.ErrorMessage)
      .Distinct()
      .ToList();


    throw new ValidationException(errorMessages);
  }
}
