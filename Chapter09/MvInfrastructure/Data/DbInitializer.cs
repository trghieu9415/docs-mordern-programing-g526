using Microsoft.Extensions.DependencyInjection;

namespace MvInfrastructure.Data;

public static class DbInitializer {
  public static async Task EnsureInitializedAsync(IServiceProvider serviceProvider) {
    await using var scope = serviceProvider.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
  }
}
