using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.Orders.GetOrder;

public record GetOrderQuery(Guid OrderId) : IRequest<TicketOrderDto>;
