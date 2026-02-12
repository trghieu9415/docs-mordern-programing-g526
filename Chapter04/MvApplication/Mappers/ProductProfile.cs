using AutoMapper;
using MvApplication.DTOs;
using MvDomain.Entities;

namespace MvApplication.Mappers;

public class ProductProfile : Profile {
  public ProductProfile() {
    CreateMap<Product, ProductDto>();
  }
}
