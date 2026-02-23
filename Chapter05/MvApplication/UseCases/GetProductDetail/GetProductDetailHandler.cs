using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProductDetail;


public class GetProductDetailHandler(IUnitOfWork uow, IMapper mapper)
  : IRequestHandler<GetProductDetailQuery, GetProductDetailResult> {

  public async Task<GetProductDetailResult> Handle(GetProductDetailQuery request, CancellationToken ct) {

    var product = await uow.Products.GetByIdWithRelationsAsync(request.Id, ct)
                  ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    var dto = mapper.Map<ProductDetailDto>(product);
    return new GetProductDetailResult(dto);
  }
}

