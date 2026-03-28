using Microsoft.JSInterop;

namespace NetWorth.Services;

public class ThemeService
{
    public bool IsDarkMode { get; private set; } = true;
    public event Action? StateChanged;

    public async Task InitializeAsync(IJSRuntime js)
    {
        var stored = await js.InvokeAsync<string?>("themeInterop.getThemePreference");
        if (stored is not null)
        {
            IsDarkMode = stored == "dark";
        }
        else
        {
            IsDarkMode = await js.InvokeAsync<bool>("themeInterop.getSystemDarkMode");
        }
    }

    public async Task ToggleAsync(IJSRuntime js)
    {
        IsDarkMode = !IsDarkMode;
        await js.InvokeVoidAsync("themeInterop.setThemePreference", IsDarkMode);
        StateChanged?.Invoke();
    }
}
