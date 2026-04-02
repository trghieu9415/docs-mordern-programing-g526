using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MvApplication.Ports;
using Polly;
using Polly.Extensions.Http;

namespace MvInfrastructure.Http;

internal static class HttpClientPolicies {
  private static readonly ConcurrentDictionary<string, IAsyncPolicy<HttpResponseMessage>> RetryPolicies = new();
  private static readonly ConcurrentDictionary<string, IAsyncPolicy<HttpResponseMessage>> CircuitBreakerPolicies = new();

  public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(IServiceProvider serviceProvider, string clientName) {
    return RetryPolicies.GetOrAdd(clientName, _ => {
      var logger = serviceProvider.GetRequiredService<IAppLogger<HttpClientPolicyMarker>>();

      return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
          retryCount: 3,
          sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
          onRetry: (outcome, delay, retryAttempt, _) => {
            var reason = outcome.Exception?.Message ?? $"HTTP {(int)outcome.Result!.StatusCode}";
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss} WRN] [{clientName}] Retry lan {retryAttempt} sau {delay.TotalSeconds}s. Ly do: {reason}");
            logger.LogSystemWarning("[{Client}] Retry lan {RetryAttempt} sau {Delay}s. Ly do: {Reason}", clientName, retryAttempt, delay.TotalSeconds, reason);
          });
    });
  }

  public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(IServiceProvider serviceProvider, string clientName) {
    return CircuitBreakerPolicies.GetOrAdd(clientName, _ => {
      var logger = serviceProvider.GetRequiredService<IAppLogger<HttpClientPolicyMarker>>();

      return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .CircuitBreakerAsync(
          handledEventsAllowedBeforeBreaking: 3,
          durationOfBreak: TimeSpan.FromSeconds(30),
          onBreak: (outcome, breakDelay) => {
            var reason = outcome.Exception?.Message ?? $"HTTP {(int)outcome.Result!.StatusCode}";
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss} WRN] [{clientName}] Circuit Breaker OPEN trong {breakDelay.TotalSeconds}s. Ly do: {reason}");
            logger.LogSystemWarning("[{Client}] Circuit Breaker OPEN trong {Delay}s. Ly do: {Reason}", clientName, breakDelay.TotalSeconds, reason);
          },
          onReset: () => {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss} INF] [{clientName}] Circuit Breaker da RESET.");
            logger.LogBusinessInformation("[{Client}] Circuit Breaker da RESET.", clientName);
          },
          onHalfOpen: () => {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss} INF] [{clientName}] Circuit Breaker chuyen sang HALF-OPEN.");
            logger.LogBusinessInformation("[{Client}] Circuit Breaker chuyen sang HALF-OPEN.", clientName);
          });
    });
  }
}

internal sealed class HttpClientPolicyMarker;
