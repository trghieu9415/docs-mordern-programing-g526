using MediatR;

namespace MvApplication.UseCases.GetProductDetail;

/// <summary>
/// Query lấy chi tiết sản phẩm kèm toàn bộ relations: Category, Tags, ProductDetail
/// Demo Eager Loading: Include tất cả navigation properties trong 1 query
/// </summary>
public record GetProductDetailQuery(Guid Id) : IRequest<GetProductDetailResult>;

