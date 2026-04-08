using MvInfrastructure;
using MvPresentation.Exceptions;
using MvPresentation.Extensions;
using MvPresentation.Filters;
using MvPresentation.Hubs;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddWebApiDefaults();
builder.Services.AddSignalRAdapters(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocument();
builder.Services.AddCors(options => {
  options.AddPolicy("SimpleClient", policy => {
    policy
      .WithOrigins("http://localhost:6001", "https://localhost:6002")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
  });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<PerformanceMonitorFilter>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("SimpleClient");

if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie API v1");
    c.DocExpansion(DocExpansion.None);
  });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints ---
app.MapControllers();
app.MapHub<CinemaHub>("/hubs/cinema");
app.MapHub<UserHub>("/hubs/notification");

app.Run();
