namespace SimpleClient.Models;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterRequest(string Username, string Password);

public sealed record AuthResponse(string Token);

public sealed record ShowtimesResponse(List<ShowtimeItem> Showtimes);

public sealed record ShowtimeItem(Guid Id, string MovieTitle, DateTime StartTime);

public sealed record BookingResponse(TicketInfo Ticket);

public sealed record TicketInfo(Guid ShowtimeId, List<string> SeatCodes, decimal TotalPrice);

public sealed class SeatStatusSnapshot {
  public List<string> BookedSeats { get; set; } = [];
  public List<string> HeldSeats { get; set; } = [];
}

public sealed class ApiProblem {
  public string? Title { get; set; }
  public string? Detail { get; set; }
}
