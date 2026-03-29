using FluentValidation;

namespace MvApplication.UseCases.Events.CreateEvent;

public class CreateEventValidator : AbstractValidator<CreateEventCommand> {
  public CreateEventValidator() {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
    RuleFor(x => x.StartAt).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
    RuleFor(x => x.TicketPrice).GreaterThan(0);
    RuleFor(x => x.Capacity).GreaterThan(0);
  }
}
