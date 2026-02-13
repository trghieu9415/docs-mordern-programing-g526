using MvApplication.Exceptions;
using MvApplication.Ports;
using MvDomain.Exceptions;
using MvInfrastructure.Exceptions;
using MvPresentation.Response;

namespace MvPresentation.Middlewares;

public class GlobalExceptionMiddleware(
  RequestDelegate next,
  IAppLogger<GlobalExceptionMiddleware> logger
) {
  public async Task InvokeAsync(HttpContext context) {
    try {
      await next(context);
    } catch (Exception ex) {
      var shouldLogToFile = ex
        is DomainException
        or AppException
        or ValidationException
        or InfrastructureException;

      if (shouldLogToFile) {
        logger.LogBusinessError(ex, "{Message}", ex.Message);
      } else {
        logger.LogSystemError(ex, "{Message}", ex.Message);
      }


      var (statusCode, responseModel) = MapException(ex);
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = statusCode;
      await context.Response.WriteAsJsonAsync(responseModel);
    }
  }

  private static (int StatusCode, object ResponseValue) MapException(Exception ex) {
    return ex switch {
      ValidationException vEx => (
        422,
        AppResponse.Fail(
          vEx.Errors,
          vEx.Errors.FirstOrDefault() ?? "Dữ liệu không hợp lệ", 422).Value!
      ),
      DomainException dEx => (400, AppResponse.Fail(dEx.Message, 400).Value!),
      InfrastructureException iEx => (500, AppResponse.Fail(iEx.Message, 500).Value!),
      AppException appEx => (appEx.StatusCode, AppResponse.Fail(appEx.Message, appEx.StatusCode).Value!),
      _ => (500, AppResponse.Fail("Lỗi hệ thống!", 500).Value!)
    };
  }
}
