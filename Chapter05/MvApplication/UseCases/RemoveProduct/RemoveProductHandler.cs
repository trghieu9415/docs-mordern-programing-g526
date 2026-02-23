using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.RemoveProduct;

public class RemoveProductHandler(IUnitOfWork uow) : IRequestHandler<RemoveProductCommand, Unit> {
  public async Task<Unit> Handle(RemoveProductCommand request, CancellationToken ct) {
    var product = await uow.Products.GetByIdWithTrackingAsync(request.Id, ct)
                  ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    uow.Products.Delete(product);
    await uow.SaveChangesAsync(ct);
    return Unit.Value;
  }
}
