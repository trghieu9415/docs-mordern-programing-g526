using Microsoft.OpenApi.Models;

namespace MvPresentation.Extensions;

public static class SwaggerExtension {
  public static IServiceCollection AddSwaggerDocument(this IServiceCollection services) {
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c => {
      c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Event Ticketing API",
        Version = "v1"
      });

      c.DescribeAllParametersInCamelCase();
      c.DocInclusionPredicate((docName, apiDesc) =>
        string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));
      c.TagActionsBy(api => [api.ActionDescriptor.RouteValues["controller"] ?? "Api"]);
    });

    return services;
  }
}
