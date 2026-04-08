using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MvApplication.Abstractions;
using MvApplication.Repositories;
using MvInfrastructure.Persistence;
using MvInfrastructure.Persistence.Repositories;
using Npgsql;

namespace MvInfrastructure.Extensions;

public static class PersistenceExtensions {
  public static IServiceCollection AddPostgresPersistence(this IServiceCollection services, IConfiguration config) {
    // Connection & DbContext
    var connectionString = config.GetConnectionString("DefaultConnection");
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();

    services.AddSingleton(dataSourceBuilder.Build());
    services.AddDbContext<AppDbContext>((serviceProvider, options) => {
      var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
      options.UseNpgsql(dataSource, builder =>
        builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
      );
      options.ConfigureWarnings(warning =>
        warning.Ignore(RelationalEventId.PendingModelChangesWarning)
      );
    });

    services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());

    // Repositories
    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    return services;
  }
}
