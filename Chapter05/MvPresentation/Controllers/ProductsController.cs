using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.CreateProduct;
using MvApplication.UseCases.GetProduct;
using MvApplication.UseCases.GetProducts;
using MvApplication.UseCases.RemoveProduct;
using MvApplication.UseCases.UpdateProduct;
using MvPresentation.Response;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class ProductsController(IMediator mediator) : ControllerBase {
  [HttpGet]
  public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query) {
    var result = await mediator.Send(query);
    return AppResponse.Success(result.Products, result.Meta);
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetProduct(Guid id) {
    var result = await mediator.Send(new GetProductQuery(id));
    return AppResponse.Success(result.Product);
  }

  [HttpPost]
  public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command) {
    var productId = await mediator.Send(command);
    return AppResponse.Success(productId, "Tạo sản phẩm thành công", 201);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command) {
    command = command with { Id = id };
    var productId = await mediator.Send(command);
    return AppResponse.Success(productId, "Cập nhật sản phẩm thành công");
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteProduct(Guid id) {
    await mediator.Send(new RemoveProductCommand(id));
    return AppResponse.Success("Xóa sản phẩm thành công");
  }
}
