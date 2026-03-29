using FluentValidation;
using MediatR;

namespace MvApplication.UseCases.ReleaseSeat;

public record ReleaseSeatCommand(Guid ShowtimeId, Guid UserId, string SeatCode) : IRequest<ReleaseSeatsResult>;

public record ReleaseSeatsResult(bool Success);

public class ReleaseSeatsValidator : AbstractValidator<ReleaseSeatCommand> {
  public ReleaseSeatsValidator() {
    RuleFor(x => x.ShowtimeId).NotEmpty().WithMessage("ShowtimeId không được để trống.");
    RuleFor(x => x.SeatCode).NotEmpty().WithMessage("Phải chọn ít nhất 1 ghế.");
  }
}
