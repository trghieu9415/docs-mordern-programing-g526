using MvApplication.DTOs;
using MvApplication.Models;

namespace MvApplication.UseCases.GetProductsWithCategory;

public record GetProductsWithCategoryResult(IList<ProductWithCategoryDto> Products, Meta Meta);

