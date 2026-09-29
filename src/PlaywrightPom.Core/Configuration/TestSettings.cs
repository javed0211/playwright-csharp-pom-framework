namespace PlaywrightPom.Core.Configuration;

public sealed class TestSettings
{
    public string EnvironmentName { get; set; } = "Development";

    public RunOptions Run { get; set; } = new();

    public WebOptions Web { get; set; } = new();

    public ApiOptions Api { get; set; } = new();

    public void EnsureRunnable()
    {
        if (string.IsNullOrWhiteSpace(Web.BaseUrl))
        {
            throw new InvalidOperationException("Web:BaseUrl is required.");
        }

        if (string.IsNullOrWhiteSpace(Api.BaseUrl))
        {
            throw new InvalidOperationException("Api:BaseUrl is required.");
        }

        if (Run.TimeoutMs <= 0 || Run.NavigationTimeoutMs <= 0 || Run.ExpectTimeoutMs <= 0)
        {
            throw new InvalidOperationException("Run timeouts must be positive.");
        }

        if (Api.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("Api:TimeoutSeconds must be positive.");
        }
    }
}

public sealed class RunOptions
{
    public string Browser { get; set; } = "chromium";

    public string? Channel { get; set; }

    public bool Headless { get; set; } = true;

    public int SlowMoMs { get; set; }

    public int TimeoutMs { get; set; } = 30_000;

    public int NavigationTimeoutMs { get; set; } = 45_000;

    public int ExpectTimeoutMs { get; set; } = 10_000;

    public int ViewportWidth { get; set; } = 1440;

    public int ViewportHeight { get; set; } = 900;

    /// <summary>on, off, or on-failure.</summary>
    public string Trace { get; set; } = "on-failure";

    public bool CaptureScreenshotOnFailure { get; set; } = true;

    public bool RecordVideo { get; set; }

    public string ArtifactRoot { get; set; } = "artifacts";

    public string LogLevel { get; set; } = "Information";
}

public sealed class WebOptions
{
    public string BaseUrl { get; set; } = "";

    public string DefaultUser { get; set; } = "";

    public string DefaultPassword { get; set; } = "";

    public string LockedOutUser { get; set; } = "";

    public string InvalidPassword { get; set; } = "";

    public string TestIdAttribute { get; set; } = "data-test";
}

public sealed class ApiOptions
{
    public string BaseUrl { get; set; } = "";

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Extra attempts after the first request for transient GET / 429 failures.</summary>
    public int RetryCount { get; set; } = 2;
}
