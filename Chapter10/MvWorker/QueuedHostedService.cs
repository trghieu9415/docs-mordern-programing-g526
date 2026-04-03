using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MvApplication.Ports;

namespace Mv.Worker;

public class QueuedHostedService(
  IBackgroundTaskQueue taskQueue,
  IServiceScopeFactory serviceScopeFactory,
  ILogger<QueuedHostedService> logger
) : BackgroundService {
  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    logger.LogInformation("Background Worker is Running!");

    while (!stoppingToken.IsCancellationRequested) {
      var workItem = await taskQueue.DequeueAsync(stoppingToken);

      try {
        using var scope = serviceScopeFactory.CreateScope();
        await workItem(stoppingToken, scope.ServiceProvider);
      } catch (Exception ex) {
        logger.LogError(ex, "Lỗi khi xử lý task ngầm!");
      }
    }
  }
}
