using FluentValidation;
using MediatR;

namespace MvApplication.UseCases.UpdateUserPoint;

public record UpdateUserPointCommand(
  Guid UserId,
  int Point
) : IRequest<UpdateUserPointResult>;

public record UpdateUserPointResult(
  Guid UserId,
  int CumulativePoint
);

public class UpdateUserPointValidator : AbstractValidator<UpdateUserPointCommand> {
  public UpdateUserPointValidator() {
    RuleFor(x => x.UserId)
      .NotEmpty().WithMessage("UserId không được để trống.");

    RuleFor(x => x.Point)
      .NotEqual(0).WithMessage("Point không được bằng 0.");
  }
}
