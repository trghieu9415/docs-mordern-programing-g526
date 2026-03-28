using FluentValidation;
using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.GetShowtimeById;

public record GetShowtimeByIdQuery(Guid Id) : IRequest<GetShowtimeByIdResult>;

public record GetShowtimeByIdResult(
  ShowtimeDto Showtime
);

public class GetShowtimeByIdValidator : AbstractValidator<GetShowtimeByIdQuery> {
  public GetShowtimeByIdValidator() {
    RuleFor(x => x.Id).NotEmpty().WithMessage("Id không được để trống bro ơi.");
  }
}
