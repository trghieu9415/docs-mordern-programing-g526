using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvPresentation.Filters;

public class PerformanceMonitorFilter(ILogger<PerformanceMonitorFilter> logger) : IAsyncActionFilter {
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
    var timer = Stopwatch.StartNew();
    await next();
    timer.Stop();
    var elapsedMilliseconds = timer.ElapsedMilliseconds;
    if (elapsedMilliseconds > 500) {
      var methodName = context.ActionDescriptor.DisplayName;
      var path = context.HttpContext.Request.Path;

      logger.LogWarning(
        "API {Method} tại {Path} xử lý quá chậm! " +
        "Thời gian: {Elapsed}ms (Vượt ngưỡng 500ms)",
        methodName, path, elapsedMilliseconds
      );
    }

    if (!context.HttpContext.Response.HasStarted) {
      context.HttpContext.Response.Headers.Append("X-Server-Execution-Time-ms", elapsedMilliseconds.ToString());
    }
  }
}
