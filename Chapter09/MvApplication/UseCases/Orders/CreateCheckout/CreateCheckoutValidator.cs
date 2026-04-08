using FluentValidation;

namespace MvApplication.UseCases.Orders.CreateCheckout;

public class CreateCheckoutValidator : AbstractValidator<CreateCheckoutCommand> {
  public CreateCheckoutValidator() {
    RuleFor(x => x.EventId).NotEmpty();
    RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
    RuleFor(x => x.Quantity).GreaterThan(0);
  }
}
