using MvApplication.DTOs;
using MvApplication.Models;

namespace MvApplication.UseCases.GetProducts;

public record GetProductsResult(IList<ProductDto> Products, Meta Meta);
