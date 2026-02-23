using MvApplication.DTOs;
using MvApplication.Models;
namespace MvApplication.UseCases.GetProductsByCategory;
public record GetProductsByCategoryResult(IList<ProductWithCategoryDto> Products, Meta Meta);
