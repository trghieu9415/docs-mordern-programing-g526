using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MvInfrastructure;
using MvInfrastructure.Data;
using MvInfrastructure.Seed;
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

using (var scope = app.Services.CreateScope()) {
  var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
  var seed = scope.ServiceProvider.GetRequiredService<LibrarySeed>();

  await dbContext.Database.MigrateAsync();
  await seed.SeedAsync();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Library API v1");
    c.DocExpansion(DocExpansion.None);
  });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
