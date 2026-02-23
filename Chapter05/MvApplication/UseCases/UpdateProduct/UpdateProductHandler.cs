﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.UpdateProduct;

public class UpdateProductHandler(IUnitOfWork uow) : IRequestHandler<UpdateProductCommand, Guid> {
  public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken ct) {

    var product = await uow.Products.GetByIdWithTrackingAsync(request.Id, ct)
                  ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    product.Update(request.Name, request.Price, request.ImageUrl, request.CategoryId);
    uow.Products.Update(product);

    try {
      await uow.SaveChangesAsync(ct);
    } catch (DbUpdateConcurrencyException) {
      throw new ConcurrencyException(
        $"Sản phẩm ID: {request.Id} đã bị thay đổi bởi người dùng khác. " +
        "Vui lòng tải lại trang và thử lại.");
    }

    return product.Id;
  }
}
