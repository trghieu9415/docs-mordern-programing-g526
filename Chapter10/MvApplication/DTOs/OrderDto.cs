using MvDomain.Enums;

namespace MvApplication.DTOs;

public record OrderDto(
  Guid Id,
  Guid UserId,
  string CustomerEmail,
  decimal TotalAmount,
  OrderStatus Status
);
