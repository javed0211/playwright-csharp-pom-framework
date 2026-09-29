using Microsoft.Playwright;
using PlaywrightPom.Core.Configuration;

namespace PlaywrightPom.Api.Client;

public sealed class ApiClientFactory
{
    private readonly IPlaywright _playwright;
    private readonly ApiOptions _options;

    public ApiClientFactory(IPlaywright playwright, ApiOptions options)
    {
        _playwright = playwright;
        _options = options;
    }

    public async Task<ApiClient> CreateAsync(string? bearerToken = null)
    {
        var headers = new Dictionary<string, string>
        {
            ["Accept"] = "application/json",
        };

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            headers["Authorization"] = $"Bearer {bearerToken}";
        }

        var context = await _playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = _options.BaseUrl,
            IgnoreHTTPSErrors = true,
            Timeout = _options.TimeoutSeconds * 1000,
            UserAgent = "PlaywrightPom/1.0",
            ExtraHTTPHeaders = headers,
        }).ConfigureAwait(false);

        return new ApiClient(context, _options);
    }
}
