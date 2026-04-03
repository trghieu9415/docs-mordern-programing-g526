using MvDomain.Base;
using MvDomain.Enums;
using MvDomain.Events;
using MvDomain.Exceptions;

namespace MvDomain.Entities;

public class Order : BaseEntity {
  private Order() {}
  public string CustomerEmail { get; private set; } = null!;
  public Guid CustomerId { get; private set; }
  public decimal TotalAmount { get; private set; }
  public OrderStatus Status { get; private set; } = OrderStatus.Pending;

  public static Order Create(Guid customerId, string customerEmail, decimal totalAmount) {
    var order = new Order {
      CustomerId = customerId,
      CustomerEmail = customerEmail,
      TotalAmount = totalAmount
    };
    order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, totalAmount, customerEmail));
    return order;
  }

  public void Confirm() {
    if (Status != OrderStatus.Pending) {
      throw new DomainException("Đơn hàng không đang ở trạng thái Chờ.");
    }

    Status = OrderStatus.Confirmed;
  }
}
