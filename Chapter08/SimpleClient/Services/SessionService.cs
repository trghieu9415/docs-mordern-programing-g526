namespace SimpleClient.Services;

public sealed class SessionService(BrowserStorageService storage) {
  private const string TokenKey = "mv-cinema-token";
  private const string UsernameKey = "mv-cinema-username";
  private bool _initialized;

  public string? Token { get; private set; }
  public string? Username { get; private set; }
  public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

  public event Action? OnChange;

  public async Task InitializeAsync() {
    if (_initialized) {
      return;
    }

    Token = await storage.GetAsync(TokenKey);
    Username = await storage.GetAsync(UsernameKey);
    _initialized = true;
    NotifyStateChanged();
  }

  public async Task SaveAuthAsync(string token, string username) {
    Token = token;
    Username = username;
    _initialized = true;

    await storage.SetAsync(TokenKey, token);
    await storage.SetAsync(UsernameKey, username);
    NotifyStateChanged();
  }

  public async Task LogoutAsync() {
    Token = null;
    Username = null;
    _initialized = true;

    await storage.RemoveAsync(TokenKey);
    await storage.RemoveAsync(UsernameKey);
    NotifyStateChanged();
  }

  private void NotifyStateChanged() {
    OnChange?.Invoke();
  }
}
