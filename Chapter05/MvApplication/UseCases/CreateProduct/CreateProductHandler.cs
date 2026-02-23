﻿using MediatR;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvApplication.UseCases.CreateProduct;

public class CreateProductHandler(IUnitOfWork uow) : IRequestHandler<CreateProductCommand, Guid> {
  public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct) {
    var product = Product.Create(request.Name, request.Price, request.ImageUrl, request.CategoryId);
    await uow.Products.AddAsync(product, ct);
    await uow.SaveChangesAsync(ct); 
    return product.Id;
  }
}
