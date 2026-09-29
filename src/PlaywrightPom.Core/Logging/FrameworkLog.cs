using PlaywrightPom.Core.Artifacts;
using PlaywrightPom.Core.Configuration;
using Serilog;
using Serilog.Events;

namespace PlaywrightPom.Core.Logging;

public static class FrameworkLog
{
    private static readonly object Gate = new();
    private static bool _ready;

    public static Serilog.ILogger Logger { get; private set; } = Serilog.Core.Logger.None;

    public static void Initialize(TestSettings settings, string runId)
    {
        lock (Gate)
        {
            if (_ready)
            {
                return;
            }

            var logDirectory = ArtifactLayout.Logs(settings);
            var level = Enum.TryParse<LogEventLevel>(settings.Run.LogLevel, true, out var parsed)
                ? parsed
                : LogEventLevel.Information;

            Logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .Enrich.WithProperty("RunId", runId)
                .Enrich.WithProperty("Environment", settings.EnvironmentName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    Path.Combine(logDirectory, "framework-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:o} [{Level:u3}] {RunId} {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Logger = Logger;
            _ready = true;
            Logger.Information(
                "Run {RunId} started. Environment={Environment} Browser={Browser} Web={Web} Api={Api}",
                runId,
                settings.EnvironmentName,
                settings.Run.Browser,
                settings.Web.BaseUrl,
                settings.Api.BaseUrl);
        }
    }

    public static void Information(string message) => Logger.Information("{Message}", SecretRedactor.Redact(message));

    public static void Warning(string message) => Logger.Warning("{Message}", SecretRedactor.Redact(message));

    public static void Error(string message) => Logger.Error("{Message}", SecretRedactor.Redact(message));

    public static void Close()
    {
        lock (Gate)
        {
            if (!_ready)
            {
                return;
            }

            Log.CloseAndFlush();
            Logger = Serilog.Core.Logger.None;
            _ready = false;
        }
    }
}
