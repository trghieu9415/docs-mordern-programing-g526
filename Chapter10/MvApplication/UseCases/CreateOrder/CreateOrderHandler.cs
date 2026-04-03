using MediatR;
using MvApplication.DTOs;
using MvApplication.Repositories;
using MvDomain.Entities;

namespace MvApplication.UseCases.CreateOrder;

public class CreateOrderHandler(
  IRepository<Order> orderRepository
) : IRequestHandler<CreateOrderCommand, CreateOrderResult> {
  public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken ct) {
    var order = Order.Create(
      request.UserId,
      request.CustomerEmail.Trim().ToLower(),
      request.TotalAmount
    );

    await orderRepository.CreateAsync(order, ct);
    var dto = new OrderDto(
      order.Id,
      order.CustomerId,
      order.CustomerEmail,
      order.TotalAmount,
      order.Status
    );

    return new CreateOrderResult(dto);
  }
}
