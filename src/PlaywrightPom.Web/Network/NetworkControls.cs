using Microsoft.Playwright;

namespace PlaywrightPom.Web.Network;

public static class NetworkControls
{
    public static Task AbortImagesAsync(IPage page) =>
        page.RouteAsync("**/*.{png,jpg,jpeg,webp,gif,svg}", route => route.AbortAsync());
}

public sealed class TrafficLog
{
    private readonly List<string> _urls = [];

    public IReadOnlyList<string> Urls => _urls;

    public void Attach(IPage page) => page.Request += (_, request) => _urls.Add(request.Url);
}
