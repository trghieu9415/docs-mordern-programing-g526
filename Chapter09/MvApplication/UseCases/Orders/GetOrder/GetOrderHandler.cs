using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.Orders.GetOrder;

public class GetOrderHandler(
  ITicketOrderRepository repository,
  IMapper mapper
) : IRequestHandler<GetOrderQuery, TicketOrderDto> {
  public async Task<TicketOrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken) {
    var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
    if (order is null) {
      throw new AppException("Khong tim thay don ve.", 404);
    }

    return mapper.Map<TicketOrderDto>(order);
  }
}
