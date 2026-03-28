using FluentValidation;
using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.BookTicket;

public record BookTicketCommand(Guid ShowtimeId, List<string> SeatCodes) : IRequest<BookTicketResult>;

public record BookTicketResult(TicketDto Ticket);

public class BookTicketValidator : AbstractValidator<BookTicketCommand> {
  public BookTicketValidator() {
    RuleFor(x => x.ShowtimeId).NotEmpty();
    RuleFor(x => x.SeatCodes).NotEmpty().WithMessage("Phải chọn ít nhất 1 ghế.");
  }
}
