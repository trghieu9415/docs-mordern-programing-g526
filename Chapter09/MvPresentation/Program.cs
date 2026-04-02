using System.Text.Json;
using System.Text.Json.Serialization;
using MvInfrastructure;
using MvInfrastructure.Data;
using MvPresentation.Extensions;
using MvPresentation.Middlewares;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogCustom();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
  .ConfigureApiBehaviorOptions(options => {
    options.SuppressModelStateInvalidFilter = true;
  })
  .AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  });

builder.Services.AddRouting(options => {
  options.LowercaseUrls = true;
  options.LowercaseQueryStrings = true;
});

builder.Services.AddSwaggerDocument();

var app = builder.Build();

await DbInitializer.EnsureInitializedAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Ticketing API v1");
    c.DocExpansion(DocExpansion.None);
  });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
