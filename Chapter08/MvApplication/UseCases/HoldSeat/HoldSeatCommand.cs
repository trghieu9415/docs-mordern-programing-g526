using FluentValidation;
using MediatR;

namespace MvApplication.UseCases.HoldSeat;

public record HoldSeatCommand(Guid ShowtimeId, Guid UserId, string SeatCode) : IRequest<HoldSeatsResult>;

public record HoldSeatsResult(bool Success);

public class HoldSeatsValidator : AbstractValidator<HoldSeatCommand> {
  public HoldSeatsValidator() {
    RuleFor(x => x.ShowtimeId).NotEmpty().WithMessage("ShowtimeId không được để trống.");
    RuleFor(x => x.SeatCode).NotEmpty().WithMessage("Phải chọn ít nhất 1 ghế.");
  }
}
