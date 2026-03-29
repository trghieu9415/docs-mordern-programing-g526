using Microsoft.AspNetCore.Mvc;
using MvApplication.DTOs;
using MvApplication.Services;
using MvPresentation.Response;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class BooksController(IBookService bookService) : ControllerBase {
  [HttpGet("get-list")]
  public async Task<IActionResult> GetList(CancellationToken ct) {
    var books = await bookService.GetListAsync(ct);
    return AppResponse.Success(books);
  }

  [HttpGet("get-detail/{id:int}")]
  public async Task<IActionResult> GetDetail(int id, CancellationToken ct) {
    var book = await bookService.GetDetailAsync(id, ct);
    return AppResponse.Success(book);
  }

  [HttpPut("update/{id:int}")]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateBookRequest request, CancellationToken ct) {
    var book = await bookService.UpdateAsync(id, request, ct);
    return AppResponse.Success(book, "C\u1eadp nh\u1eadt s\u00e1ch th\u00e0nh c\u00f4ng");
  }
}
