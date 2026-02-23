using AutoMapper;
using MvApplication.DTOs;
using MvDomain.Entities;

namespace MvApplication.Mappers;

public class ProductProfile : Profile {
  public ProductProfile() {

    CreateMap<Product, ProductDto>();
    
    CreateMap<Product, ProductWithCategoryDto>()
      .ConstructUsing(src => new ProductWithCategoryDto(
        src.Id,
        src.Name,
        src.Price,
        src.Stock,
        src.ImageUrl,
        src.Category != null ? src.Category.Name : null
      ));
    
    CreateMap<Product, ProductDetailDto>()
      .ConstructUsing(src => new ProductDetailDto(
        src.Id,
        src.Name,
        src.Price,
        src.Stock,
        src.ImageUrl,
        src.Category != null ? src.Category.Name : null,
        src.Tags.Select(t => t.Name).ToList(),
        src.Detail != null ? src.Detail.Description : null,
        src.Detail != null ? src.Detail.Specification : null
      ));
  }
}
