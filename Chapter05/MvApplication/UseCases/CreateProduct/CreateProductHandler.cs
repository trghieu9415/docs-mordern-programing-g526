using MediatR;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvApplication.UseCases.CreateProduct;

public class CreateProductHandler(IProductManager productManager)
  : IRequestHandler<CreateProductCommand, Guid> {
  public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken) {
    var product = Product.Create(request.Name, request.Price, request.ImageUrl);
    await productManager.AddAsync(product, cancellationToken);
    return product.Id;
  }
}
