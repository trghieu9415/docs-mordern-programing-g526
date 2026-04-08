using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProductPurchaseProof;

public class GetProductPurchaseProofHandler(IProductManager productManager)
  : IRequestHandler<GetProductPurchaseProofQuery, GetProductPurchaseProofResult> {
  public async Task<GetProductPurchaseProofResult> Handle(GetProductPurchaseProofQuery request, CancellationToken ct) {
    var product =
      await productManager.GetByIdAsync(request.ProductId, ct)
      ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.ProductId}", 404);

    var orders = await productManager.GetOrdersByProductIdAsync(request.ProductId, ct);
    var dtos = orders
      .Select(o => new OrderSummaryDto(o.Id, o.ProductId, o.UserId, o.Quantity, o.CreatedAt))
      .ToList();

    return new GetProductPurchaseProofResult(
      product.Id,
      product.Name,
      product.AvailableStock,
      dtos.Count,
      dtos
    );
  }
}
