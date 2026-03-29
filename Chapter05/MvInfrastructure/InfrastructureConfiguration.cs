using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MvApplication;
using MvApplication.Behaviors;
using MvApplication.Ports;
using MvApplication.Repositories;
using MvApplication.Services;
using MvInfrastructure.Adapters;
using MvInfrastructure.Data;
using MvInfrastructure.Repositories;
using MvInfrastructure.Seed;

namespace MvInfrastructure;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) {
    services.AddApplication();

    var connectionString =
      config.GetConnectionString("LibraryDb")
      ?? "Host=localhost;Port=5432;Database=elibrary_db;Username=postgres;Password=postgres";

    services.AddDbContext<LibraryDbContext>(options => {
      options.UseNpgsql(connectionString);
    });

    services.AddSingleton(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
    services.AddScoped<IBookReadRepository, BookReadRepository>();
    services.AddScoped<IBookRepository, BookRepository>();
    services.AddScoped<ICategoryRepository, CategoryRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<IBookService, BookService>();
    services.AddScoped<LibrarySeed>();

    return services;
  }

  private static IServiceCollection AddApplication(this IServiceCollection services) {
    var applicationAssembly = typeof(IApplicationMarker).Assembly;

    services.AddMediatR(cfg => {
      cfg.RegisterServicesFromAssembly(applicationAssembly);
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    services.AddValidatorsFromAssembly(applicationAssembly);
    return services;
  }
}
