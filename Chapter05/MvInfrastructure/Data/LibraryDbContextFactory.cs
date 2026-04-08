using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MvInfrastructure.Data;

public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext> {
  public LibraryDbContext CreateDbContext(string[] args) {
    var presentationPath = Path.GetFullPath(
      Path.Combine(Directory.GetCurrentDirectory(), "..", "MvPresentation")
    );

    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.Exists(presentationPath) ? presentationPath : Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: true)
      .AddJsonFile("appsettings.Development.json", optional: true)
      .AddEnvironmentVariables()
      .Build();

    var connectionString =
      configuration.GetConnectionString("LibraryDb")
      ?? "Host=localhost;Port=5432;Database=elibrary_db;Username=postgres;Password=postgres";

    var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
    optionsBuilder.UseNpgsql(connectionString);

    return new LibraryDbContext(optionsBuilder.Options);
  }
}
