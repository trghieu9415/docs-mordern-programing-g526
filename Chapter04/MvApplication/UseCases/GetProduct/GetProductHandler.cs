using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProduct;

public class GetProductHandler(
  IProductManager productManager,
  IMapper mapper
) : IRequestHandler<GetProductQuery, GetProductResult> {
  public async Task<GetProductResult> Handle(GetProductQuery request, CancellationToken ct) {
    var product =
      await productManager.GetByIdAsync(request.Id, ct)
      ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    var dto = mapper.Map<ProductDto>(product);
    return new GetProductResult(dto);
  }
}
