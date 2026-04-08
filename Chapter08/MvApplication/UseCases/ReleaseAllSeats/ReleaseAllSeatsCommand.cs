using FluentValidation;
using MediatR;

namespace MvApplication.UseCases.ReleaseAllSeats;

public record ReleaseAllSeatsCommand(Guid ShowtimeId, Guid UserId) : IRequest<ReleaseAllSeatsResult>;

public record ReleaseAllSeatsResult(List<string> ReleasedSeats);

public class ReleaseAllSeatsValidator : AbstractValidator<ReleaseAllSeatsCommand> {
  public ReleaseAllSeatsValidator() {
    RuleFor(x => x.ShowtimeId).NotEmpty().WithMessage("ShowtimeId không được để trống.");
  }
}
