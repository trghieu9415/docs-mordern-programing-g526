using MvDomain.Exceptions;

namespace MvDomain.Entities;

public class TicketOrder {
  private TicketOrder() {}

  public Guid Id { get; private set; } = Guid.NewGuid();
  public Guid EventId { get; private set; }
  public string EventName { get; private set; } = null!;
  public string CustomerEmail { get; private set; } = null!;
  public int Quantity { get; private set; }
  public decimal TotalAmount { get; private set; }
  public PaymentProvider PaymentProvider { get; private set; }
  public PaymentState Status { get; private set; } = PaymentState.Pending;
  public string? PaymentUrl { get; private set; }
  public string? GatewayReferenceId { get; private set; }
  public string? GatewayTransactionId { get; private set; }
  public string? TicketCode { get; private set; }
  public string? FailureReason { get; private set; }
  public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
  public DateTime? CompletedAt { get; private set; }

  public static TicketOrder Create(
    Guid eventId,
    string eventName,
    string customerEmail,
    int quantity,
    decimal totalAmount,
    PaymentProvider paymentProvider
  ) {
    if (quantity <= 0) {
      throw new DomainException("So luong ve phai lon hon 0.");
    }

    if (totalAmount <= 0) {
      throw new DomainException("Tong tien phai lon hon 0.");
    }

    return new TicketOrder {
      EventId = eventId,
      EventName = eventName,
      CustomerEmail = customerEmail,
      Quantity = quantity,
      TotalAmount = totalAmount,
      PaymentProvider = paymentProvider
    };
  }

  public void SetCheckout(string paymentUrl, string? gatewayReferenceId) {
    PaymentUrl = paymentUrl;
    GatewayReferenceId = gatewayReferenceId;
  }

  public void MarkAsPaid(string gatewayTransactionId, string ticketCode) {
    if (Status == PaymentState.Paid) {
      return;
    }

    Status = PaymentState.Paid;
    GatewayTransactionId = gatewayTransactionId;
    TicketCode = ticketCode;
    FailureReason = null;
    CompletedAt = DateTime.UtcNow;
  }

  public void MarkAsFailed(string? reason) {
    if (Status == PaymentState.Paid) {
      return;
    }

    Status = PaymentState.Failed;
    FailureReason = reason;
    CompletedAt = DateTime.UtcNow;
  }

  public bool IsFinalized() {
    return Status == PaymentState.Paid;
  }

  public bool IsPending() {
    return Status == PaymentState.Pending;
  }
}

public enum PaymentProvider {
  Stripe,
  PayPal
}

public enum PaymentState {
  Pending,
  Paid,
  Failed
}
