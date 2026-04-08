using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.GetOrders;

public record GetOrdersQuery : IRequest<GetOrdersResult>;

public record GetOrdersResult(List<OrderDto> Orders);
