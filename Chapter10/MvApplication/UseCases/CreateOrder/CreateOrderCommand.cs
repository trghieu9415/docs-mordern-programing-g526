using FluentValidation;
using MediatR;
using MvApplication.Abstractions;
using MvApplication.DTOs;

namespace MvApplication.UseCases.CreateOrder;

public record CreateOrderCommand(
  Guid UserId,
  string CustomerEmail,
  decimal TotalAmount
) : IRequest<CreateOrderResult>, ITransactional;

public record CreateOrderResult(OrderDto Order);

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand> {
  public CreateOrderValidator() {
    RuleFor(x => x.CustomerEmail)
      .Cascade(CascadeMode.Stop)
      .NotEmpty().WithMessage("Email không được để trống.")
      .MaximumLength(255)
      .EmailAddress().WithMessage("Email không hợp lệ.");

    RuleFor(x => x.TotalAmount)
      .GreaterThan(0).WithMessage("TotalAmount phải lớn hơn 0.");
  }
}
