using MvDomain.Entities;

namespace MvApplication.DTOs;

public record TicketOrderDto(
  Guid Id,
  Guid EventId,
  string EventName,
  string CustomerEmail,
  int Quantity,
  decimal TotalAmount,
  PaymentProvider PaymentProvider,
  PaymentState Status,
  string? PaymentUrl,
  string? GatewayReferenceId,
  string? GatewayTransactionId,
  string? TicketCode,
  string? FailureReason,
  DateTime CreatedAt,
  DateTime? CompletedAt
);
