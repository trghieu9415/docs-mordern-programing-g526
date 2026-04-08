using FluentValidation;

namespace MvApplication.UseCases.PurchaseProduct;

public class PurchaseProductValidator : AbstractValidator<PurchaseProductCommand> {
  public PurchaseProductValidator() {
    RuleFor(x => x.ProductId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty().MaximumLength(100);
    RuleFor(x => x.Quantity).GreaterThan(0);
  }
}
