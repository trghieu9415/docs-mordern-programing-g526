using FluentValidation;
using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.BookSeats;

public record BookSeatsCommand(Guid ShowtimeId, Guid UserId) : IRequest<BookSeatsResult>;

public record BookSeatsResult(TicketDto Ticket);

public class BookSeatsValidator : AbstractValidator<BookSeatsCommand> {
  public BookSeatsValidator() {
    RuleFor(x => x.ShowtimeId).NotEmpty().WithMessage("ShowtimeId không được để trống.");
  }
}
