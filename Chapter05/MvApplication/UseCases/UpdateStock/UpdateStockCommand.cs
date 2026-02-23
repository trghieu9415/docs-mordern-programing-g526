using MediatR;

namespace MvApplication.UseCases.UpdateStock;


public record UpdateStockCommand(Guid ProductId, int Quantity) : IRequest<int>;
