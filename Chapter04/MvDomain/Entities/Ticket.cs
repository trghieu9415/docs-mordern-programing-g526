using MvDomain.Base;

namespace MvDomain.Entities;

public class Ticket : BaseEntity {
  private Ticket() {}
  public Guid ShowtimeId { get; private set; }
  public List<string> SeatCodes { get; private set; } = [];
  public decimal TotalPrice { get; private set; }

  public static Ticket Create(Guid showtimeId, List<string> seatCodes, decimal totalPrice) {
    return new Ticket {
      ShowtimeId = showtimeId,
      SeatCodes = seatCodes,
      TotalPrice = totalPrice
    };
  }
}
