using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProduct;

public class GetProductHandler(IUnitOfWork uow, IMapper mapper)
  : IRequestHandler<GetProductQuery, GetProductResult> {
  public async Task<GetProductResult> Handle(GetProductQuery request, CancellationToken ct) {
    var product = await uow.Products.GetByIdAsync(request.Id, ct)
                  ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    var dto = mapper.Map<ProductDto>(product);
    return new GetProductResult(dto);
  }
}
