using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProduct;

public class GetProductHandler(
  IProductManager productManager,
  IMapper mapper
) : IRequestHandler<GetProductQuery, GetProductResult> {
  public async Task<GetProductResult> Handle(GetProductQuery request, CancellationToken cancellationToken) {
    var product = await productManager.GetByIdAsync(request.Id, cancellationToken);
    var dto = mapper.Map<ProductDto>(product);
    return new GetProductResult(dto);
  }
}
