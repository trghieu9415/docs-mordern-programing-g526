using MediatR;
using MvApplication.DTOs;
using MvApplication.Repositories;
using MvDomain.Entities;

namespace MvApplication.UseCases.GetOrders;

public class GetOrdersHandler(IRepository<Order> orderRepository) : IRequestHandler<GetOrdersQuery, GetOrdersResult> {
  public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken) {
    var orders = await orderRepository.GetAsync(ct: cancellationToken);
    var dtos = orders.Select(o => new OrderDto(o.Id, o.CustomerId, o.CustomerEmail, o.TotalAmount, o.Status)).ToList();
    return new GetOrdersResult(dtos);
  }
}
