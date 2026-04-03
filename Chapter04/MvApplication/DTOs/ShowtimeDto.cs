namespace MvApplication.DTOs;

public record ShowtimeDto(
  Guid Id,
  string MovieTitle,
  DateTime StartTime,
  List<string> BookedSeats
);
