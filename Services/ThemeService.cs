using Microsoft.JSInterop;

namespace SudokuBlazor.Services;

public class ThemeService(IJSRuntime js)
{
  private readonly IJSRuntime _js = js;

  public ValueTask<string?> GetStoredOrSystemThemeAsync()
    => _js.InvokeAsync<string?>("themeManager.getEffectiveTheme");

  public ValueTask SetThemeAsync(string? tag)
    => _js.InvokeVoidAsync("themeManager.setTheme", tag);

  public ValueTask RegisterSystemChangeCallbackAsync<T>(DotNetObjectReference<T> dotNetRef, string callbackMethod)
    where T : class
    => _js.InvokeVoidAsync("themeManager.onSystemThemeChange", dotNetRef, callbackMethod);

  public ValueTask UnregisterSystemChangeCallbackAsync<T>(DotNetObjectReference<T> dotNetRef)
    where T : class
    => _js.InvokeVoidAsync("themeManager.removeSystemThemeChangeCallback", dotNetRef);
}
