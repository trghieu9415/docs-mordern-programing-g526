using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.UpdateStock;

public class UpdateStockHandler(IUnitOfWork uow) : IRequestHandler<UpdateStockCommand, int> {
  public async Task<int> Handle(UpdateStockCommand request, CancellationToken ct) {
    
    await using var transaction = await uow.BeginTransactionAsync(ct);

    try {
      var product = await uow.Products.GetByIdWithLockAsync(request.ProductId, ct)
                    ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.ProductId}", 404);
      
      product.UpdateStock(request.Quantity);
      uow.Products.Update(product);
      
      await uow.SaveChangesAsync(ct);
      await transaction.CommitAsync(ct);

      return product.Stock;

    } catch {
      await transaction.RollbackAsync(ct);
      throw;
    }
  }
}
