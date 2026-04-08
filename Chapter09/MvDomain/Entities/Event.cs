using MvDomain.Exceptions;

namespace MvDomain.Entities;

public class Event {
  private Event() {}

  public Guid Id { get; private set; } = Guid.NewGuid();
  public string Name { get; private set; } = null!;
  public string Description { get; private set; } = null!;
  public string Venue { get; private set; } = null!;
  public DateTime StartAt { get; private set; }
  public decimal TicketPrice { get; private set; }
  public int Capacity { get; private set; }
  public int AvailableTickets { get; private set; }
  public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

  public static Event Create(
    string name,
    string description,
    string venue,
    DateTime startAt,
    decimal ticketPrice,
    int capacity
  ) {
    if (capacity <= 0) {
      throw new DomainException("Suc chua phai lon hon 0.");
    }

    if (ticketPrice <= 0) {
      throw new DomainException("Gia ve phai lon hon 0.");
    }

    return new Event {
      Name = name,
      Description = description,
      Venue = venue,
      StartAt = startAt,
      TicketPrice = ticketPrice,
      Capacity = capacity,
      AvailableTickets = capacity
    };
  }

  public void ReserveTickets(int quantity) {
    if (quantity <= 0) {
      throw new DomainException("So luong ve phai lon hon 0.");
    }

    if (AvailableTickets < quantity) {
      throw new DomainException("Khong du so luong ve trong.");
    }

    AvailableTickets -= quantity;
  }

  public void ReleaseTickets(int quantity) {
    if (quantity <= 0) {
      throw new DomainException("So luong ve phai lon hon 0.");
    }

    AvailableTickets = Math.Min(Capacity, AvailableTickets + quantity);
  }
}
