using System.Text.Json;
using Microsoft.Playwright;
using Polly;
using PlaywrightPom.Api.Json;
using PlaywrightPom.Core.Configuration;
using PlaywrightPom.Core.Diagnostics;
using PlaywrightPom.Core.Logging;
using PlaywrightPom.Core.Resilience;

namespace PlaywrightPom.Api.Client;

public sealed class ApiClient : IAsyncDisposable
{
    private readonly IAPIRequestContext _context;
    private readonly ApiOptions _options;
    private readonly ResiliencePipeline _pipeline;

    public ApiClient(IAPIRequestContext context, ApiOptions options)
    {
        _context = context;
        _options = options;
        _pipeline = TransientRetry.Create(options.RetryCount);
    }

    public Task<ApiResponse<T>> GetAsync<T>(string path) => SendAsync<T>("GET", path);

    public Task<ApiResponse<T>> PostAsync<T>(string path, object body) => SendAsync<T>("POST", path, body);

    public async Task<ApiResponse<T>> SendAsync<T>(string method, string path, object? body = null)
    {
        var response = await _pipeline.ExecuteAsync(async _ =>
        {
            IAPIResponse http;
            try
            {
                http = await SendOnceAsync(method, path, body).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsTransportFailure(ex))
            {
                throw new TransientRequestException($"Transient failure calling {method} {path}.", ex);
            }

            if (ShouldRetry(method, http.Status))
            {
                throw new TransientRequestException($"HTTP {http.Status} from {method} {path}.");
            }

            return http;
        }).ConfigureAwait(false);

        var raw = await response.TextAsync().ConfigureAwait(false);
        TestSession.Steps.Info($"{method} {path} -> {response.Status}");
        FrameworkLog.Information($"{method} {path} -> {response.Status} {Truncate(SecretRedactor.Redact(raw))}");
        return new ApiResponse<T>
        {
            Status = response.Status,
            Raw = raw,
            Body = TryDeserialize<T>(raw),
            Headers = response.Headers,
        };
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync().ConfigureAwait(false);

    private async Task<IAPIResponse> SendOnceAsync(string method, string path, object? body)
    {
        var options = new APIRequestContextOptions
        {
            Timeout = _options.TimeoutSeconds * 1000,
            Headers = new Dictionary<string, string>
            {
                ["X-Correlation-Id"] = TestSession.CorrelationId,
            },
            DataObject = body,
        };

        return method.ToUpperInvariant() switch
        {
            "GET" => await _context.GetAsync(path, options).ConfigureAwait(false),
            "POST" => await _context.PostAsync(path, options).ConfigureAwait(false),
            "PUT" => await _context.PutAsync(path, options).ConfigureAwait(false),
            "PATCH" => await _context.PatchAsync(path, options).ConfigureAwait(false),
            "DELETE" => await _context.DeleteAsync(path, options).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unsupported HTTP method."),
        };
    }

    private static bool ShouldRetry(string method, int status) =>
        status is 408 or 429 or 500 or 502 or 503 or 504 &&
        (method.Equals("GET", StringComparison.OrdinalIgnoreCase) || status == 429);

    private static bool IsTransportFailure(Exception ex) =>
        ex is TimeoutException
        || ex.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("net::ERR_", StringComparison.OrdinalIgnoreCase);

    private static T? TryDeserialize<T>(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return default;
        }

        var trimmed = raw.TrimStart();
        if (!trimmed.StartsWith('{') && !trimmed.StartsWith('['))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(raw, JsonDefaults.Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static string Truncate(string value) => value.Length <= 400 ? value : value[..400];
}
