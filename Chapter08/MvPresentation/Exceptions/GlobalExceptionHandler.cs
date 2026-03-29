using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvApplication.Exceptions;
using MvDomain.Exceptions;

namespace MvPresentation.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler {
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken
  ) {
    logger.LogError(exception, "Đã xảy ra lỗi: {Message}", exception.Message);

    var problemDetails = new ProblemDetails {
      Instance = httpContext.Request.Path,
      Title = "Đã xảy ra lỗi nghiệp vụ",
      Detail = exception.Message
    };

    switch (exception) {
      case SeatAlreadyBookedException seatEx:
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Title = "Ghế đã được đặt";
        problemDetails.Extensions.Add("conflictingSeats", seatEx.ConflictingSeats);
      break;

      case WorkflowException wfEx:
        httpContext.Response.StatusCode = wfEx.StatusCode;
        problemDetails.Status = wfEx.StatusCode;
        problemDetails.Title = wfEx.Message;
      break;

      case ArgumentException:
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Title = "Dữ liệu không hợp lệ";
      break;

      case KeyNotFoundException:
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        problemDetails.Status = StatusCodes.Status404NotFound;
        problemDetails.Title = "Không tìm thấy tài nguyên";
      break;

      default:
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        problemDetails.Status = StatusCodes.Status500InternalServerError;
        problemDetails.Title = "Lỗi máy chủ";
        problemDetails.Detail = "Liên hệ với Admin để kiểm tra!";
      break;
    }

    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    return true;
  }
}
