using MvApplication.Configs.Options;
using MvApplication.Ports;

namespace MvInfrastructure.Adapters;

public class TicketPriceCalculator(CinemaSettings settings) : ITicketPriceCalculator {
  public decimal Calculate(DateTime showTime) {
    var isWeekend = showTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    return isWeekend ? settings.MaxTicketsPerBooking + settings.WeekendSurcharge : settings.WeekendSurcharge;
  }
}
