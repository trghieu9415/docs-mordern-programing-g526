using System.Text.Json;
using System.Text.Json.Serialization;
using MvInfrastructure;
using MvInfrastructure.Persistence;
using MvInfrastructure.Seed;
using MvPresentation.Extensions;
using MvPresentation.Middlewares;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogCustom();

// Infrastructure with Identity and JWT
builder.Services.AddInfrastructure(builder.Configuration);

// Presentation with JWT Authentication
builder.Services.AddPresentationInfrastructure(builder.Configuration);

builder.Services.AddControllers()
  .ConfigureApiBehaviorOptions(options =>
  {
    options.SuppressModelStateInvalidFilter = true;
  })
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  });

builder.Services.AddRouting(options =>
{
  options.LowercaseUrls = true;
  options.LowercaseQueryStrings = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
  {
    Title = "Identity & JWT API",
    Version = "v1",
    Description = "User authentication and authorization API"
  });

  // Add JWT authentication to Swagger
  c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
  {
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    Name = "Authorization",
    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });

  c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
  {
    {
      new Microsoft.OpenApi.Models.OpenApiSecurityScheme
      {
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
          Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      Array.Empty<string>()
    }
  });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  var migrated = false;
  Exception? lastException = null;

  for (var attempt = 1; attempt <= 10 && !migrated; attempt++)
  {
    try
    {
      dbContext.Database.Migrate();
      migrated = true;
    }
    catch (Exception ex) when (attempt < 10)
    {
      lastException = ex;
      await Task.Delay(TimeSpan.FromSeconds(3));
    }
    catch (Exception ex)
    {
      lastException = ex;
    }
  }

  if (!migrated)
  {
    throw lastException ?? new InvalidOperationException("Không thể migrate database");
  }
}

if (args.Contains("--seed-data", StringComparer.OrdinalIgnoreCase))
{
  using var scope = app.Services.CreateScope();
  await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
  Console.WriteLine("Seed data completed.");
  return;
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity & JWT API v1");
    c.DocExpansion(DocExpansion.None);
  });
}

app.UseHttpsRedirection();

// Authentication & Authorization - Thứ tự quan trọng!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
