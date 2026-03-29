using Microsoft.JSInterop;

namespace SimpleClient.Services;

public sealed class BrowserStorageService(IJSRuntime jsRuntime) {
  public ValueTask<string?> GetAsync(string key) {
    return jsRuntime.InvokeAsync<string?>("mvCinemaStorage.get", key);
  }

  public ValueTask SetAsync(string key, string value) {
    return jsRuntime.InvokeVoidAsync("mvCinemaStorage.set", key, value);
  }

  public ValueTask RemoveAsync(string key) {
    return jsRuntime.InvokeVoidAsync("mvCinemaStorage.remove", key);
  }
}
