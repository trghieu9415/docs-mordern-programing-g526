using System.Text.Json;
using System.Text.Json.Serialization;
using MvInfrastructure;
using MvPresentation.Extensions;
using MvPresentation.Middlewares;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogCustom();

// Infrastructure with Identity and JWT
builder.Services.AddInfrastructure(builder.Configuration);

// Presentation with JWT Authentication
builder.Services.AddPresentationInfrastructure();

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
