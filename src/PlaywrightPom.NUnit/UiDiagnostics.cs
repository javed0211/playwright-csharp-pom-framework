using Microsoft.Playwright;

namespace PlaywrightPom.NUnit;

public sealed class UiDiagnostics
{
    private readonly List<string> _browserLogs = [];
    private bool _tracing;

    public IReadOnlyList<string> BrowserLogs => _browserLogs;

    public void Attach(IPage page)
    {
        page.Console += (_, message) =>
        {
            if (message.Type is "error" or "warning")
            {
                _browserLogs.Add($"{message.Type}: {message.Text}");
            }
        };

        page.PageError += (_, error) => _browserLogs.Add($"pageerror: {error}");
    }

    public async Task StartTraceAsync(IBrowserContext context, string title)
    {
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Title = title,
            Screenshots = true,
            Snapshots = true,
            Sources = true,
        }).ConfigureAwait(false);
        _tracing = true;
    }

    public async Task<(string? Screenshot, string? Trace)> StopAsync(
        IPage? page,
        IBrowserContext? context,
        bool saveScreenshot,
        bool saveTrace,
        string screenshotPath,
        string tracePath)
    {
        string? screenshot = null;
        string? trace = null;

        if (saveScreenshot && page is not null)
        {
            try
            {
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true,
                }).ConfigureAwait(false);
                screenshot = screenshotPath;
            }
            catch (Exception ex)
            {
                _browserLogs.Add($"screenshot failed: {ex.Message}");
            }
        }

        if (_tracing && context is not null)
        {
            try
            {
                if (saveTrace)
                {
                    await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath }).ConfigureAwait(false);
                    trace = tracePath;
                }
                else
                {
                    await context.Tracing.StopAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _browserLogs.Add($"trace failed: {ex.Message}");
            }

            _tracing = false;
        }

        return (screenshot, trace);
    }
}
