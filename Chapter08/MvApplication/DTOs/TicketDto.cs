namespace MvApplication.DTOs;

public record TicketDto(Guid ShowtimeId, List<string> SeatCodes, decimal TotalPrice);
