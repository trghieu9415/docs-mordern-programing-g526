using MediatR;
using Microsoft.Extensions.Options;
using MvApplication.Exceptions;
using MvApplication.Options;
using MvApplication.Ports;

namespace MvApplication.UseCases.PurchaseProduct;

public class PurchaseProductHandler(
  IProductManager productManager,
  ICacheStorage cache,
  IOptions<FlashSaleDemoOptions> demoOptions
) : IRequestHandler<PurchaseProductCommand, PurchaseProductResult> {
  public async Task<PurchaseProductResult> Handle(PurchaseProductCommand request, CancellationToken ct) {
    var delayMs = demoOptions.Value.PurchaseHandlerDelayMs;
    if (delayMs > 0) {
      await Task.Delay(delayMs, ct);
    }

    var product =
      await productManager.GetByIdAsync(request.ProductId, ct)
      ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.ProductId}", 404);

    if (product.AvailableStock < request.Quantity) {
      throw new AppException("Sản phẩm đã hết hàng hoặc không đủ tồn kho.", 400);
    }

    product.UpdateStock(-request.Quantity);
    var order = await productManager.CreateOrderAsync(product.Id, request.UserId, request.Quantity, ct);

    // Đồng bộ cache sau khi cập nhật DB thành công.
    await cache.RemoveAsync($"product:{request.ProductId}", ct);
    await cache.RemoveAsync("products:first20", ct);

    return new PurchaseProductResult(order.Id, request.ProductId, request.Quantity, product.AvailableStock);
  }
}
