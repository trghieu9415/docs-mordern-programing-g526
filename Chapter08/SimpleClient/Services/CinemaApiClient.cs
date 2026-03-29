using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using SimpleClient.Models;

namespace SimpleClient.Services;

public sealed class CinemaApiClient(HttpClient httpClient, IConfiguration configuration) {
  private readonly string _apiBaseUrl = (configuration["ApiBaseUrl"] ?? "https://localhost:7225").TrimEnd('/');
  private readonly decimal _basePrice = configuration.GetValue<decimal?>("TicketPricing:BasePrice") ?? 50000m;
  private readonly decimal _weekendSurcharge = configuration.GetValue<decimal?>("TicketPricing:WeekendSurcharge") ?? 20000m;

  public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) {
    return await PostAsync<LoginRequest, AuthResponse>("api/auth/login", request, null, ct);
  }

  public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default) {
    return await PostAsync<RegisterRequest, AuthResponse>("api/auth/register", request, null, ct);
  }

  public async Task<IReadOnlyList<ShowtimeItem>> GetShowtimesAsync(CancellationToken ct = default) {
    var response = await httpClient.GetAsync("api/showtimes", ct);
    await EnsureSuccessAsync(response, ct);
    var data = await response.Content.ReadFromJsonAsync<ShowtimesResponse>(cancellationToken: ct);
    return data?.Showtimes ?? [];
  }

  public async Task<TicketInfo> BookSeatsAsync(Guid showtimeId, string token, CancellationToken ct = default) {
    var request = new HttpRequestMessage(HttpMethod.Post, "api/bookings/book") {
      Content = JsonContent.Create(showtimeId)
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await httpClient.SendAsync(request, ct);
    await EnsureSuccessAsync(response, ct);
    var data = await response.Content.ReadFromJsonAsync<BookingResponse>(cancellationToken: ct);
    return data?.Ticket ?? throw new InvalidOperationException("Không nhận được thông tin vé từ máy chủ.");
  }

  public HubConnection CreateCinemaHubConnection(string token) {
    return new HubConnectionBuilder()
      .WithUrl($"{_apiBaseUrl}/hubs/cinema", options => {
        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
      })
      .WithAutomaticReconnect()
      .Build();
  }

  public decimal EstimateTicketTotal(DateTime startTime, int seatCount) {
    if (seatCount <= 0) {
      return 0;
    }

    var unitPrice = _basePrice;
    if (startTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) {
      unitPrice += _weekendSurcharge;
    }

    return unitPrice * seatCount;
  }

  private async Task<TResponse> PostAsync<TRequest, TResponse>(
    string url,
    TRequest payload,
    string? token,
    CancellationToken ct
  ) {
    var request = new HttpRequestMessage(HttpMethod.Post, url) {
      Content = JsonContent.Create(payload)
    };

    if (!string.IsNullOrWhiteSpace(token)) {
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    var response = await httpClient.SendAsync(request, ct);
    await EnsureSuccessAsync(response, ct);

    var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    return data ?? throw new InvalidOperationException("Máy chủ trả về dữ liệu không hợp lệ.");
  }

  private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct) {
    if (response.IsSuccessStatusCode) {
      return;
    }

    ApiProblem? problem = null;
    try {
      problem = await response.Content.ReadFromJsonAsync<ApiProblem>(cancellationToken: ct);
    } catch (NotSupportedException) {
    } catch (JsonException) {
    }

    if (!string.IsNullOrWhiteSpace(problem?.Detail)) {
      throw new InvalidOperationException(problem.Detail);
    }

    if (!string.IsNullOrWhiteSpace(problem?.Title)) {
      throw new InvalidOperationException(problem.Title);
    }

    var raw = await response.Content.ReadAsStringAsync(ct);
    if (!string.IsNullOrWhiteSpace(raw)) {
      throw new InvalidOperationException(raw.Trim('\"'));
    }

    throw new InvalidOperationException("Không thể xử lý yêu cầu lúc này.");
  }
}
