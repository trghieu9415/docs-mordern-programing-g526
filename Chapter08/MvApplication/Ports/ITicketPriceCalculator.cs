namespace MvApplication.Ports;

public interface ITicketPriceCalculator {
  decimal Calculate(DateTime showTime);
}
