using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PlaywrightPom.Core.Composition;
using PlaywrightPom.Core.Configuration;
using PlaywrightPom.Core.Diagnostics;
using PlaywrightPom.Core.Logging;
using PlaywrightPom.Reporting;

namespace PlaywrightPom.NUnit;

public static class FrameworkRuntime
{
    private static readonly object Gate = new();
    private static bool _started;
    private static bool _exitHooked;
    private static string _suite = "tests";

    public static string RunId { get; private set; } = "";

    public static TestSettings Settings { get; private set; } = new();

    public static ServiceProvider? Services { get; private set; }

    public static void EnsureStarted(string suite)
    {
        lock (Gate)
        {
            if (_started)
            {
                return;
            }

            RunId = Guid.NewGuid().ToString("N");
            _suite = suite;
            Settings = TestConfiguration.Load();
            Settings.EnsureRunnable();
            TestConfiguration.Use(Settings);
            ApplyBrowserEnvironment(Settings);
            FrameworkLog.Initialize(Settings, RunId);
            TestSession.UseKeyResolver(CurrentTestKey);
            Services = FrameworkComposer.Build(Settings);
            ReportCollector.Instance.Begin(suite, Settings, RunId);
            if (!_exitHooked)
            {
                AppDomain.CurrentDomain.ProcessExit += (_, _) => Finish(_suite);
                _exitHooked = true;
            }

            _started = true;
        }
    }

    public static void Finish(string suite)
    {
        string? report;
        lock (Gate)
        {
            if (!_started)
            {
                return;
            }

            report = ReportCollector.Instance.Flush(Settings, suite);
            Services?.Dispose();
            Services = null;
            FrameworkLog.Information(report is null ? "Run finished." : $"HTML report: {report}");
            FrameworkLog.Close();
            _started = false;
        }

        if (!string.IsNullOrWhiteSpace(report))
        {
            Console.WriteLine($"HTML report: {report}");
        }
    }

    private static string? CurrentTestKey()
    {
        try
        {
            var id = TestContext.CurrentContext.Test.ID;
            return string.IsNullOrEmpty(id) ? null : id;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void ApplyBrowserEnvironment(TestSettings settings)
    {
        var existing = Environment.GetEnvironmentVariable("BROWSER");
        var valid = existing is "chromium" or "firefox" or "webkit";
        if (!valid)
        {
            Environment.SetEnvironmentVariable("BROWSER", settings.Run.Browser);
        }
        else
        {
            settings.Run.Browser = existing!;
        }

        if (Environment.GetEnvironmentVariable("HEADED") == "1")
        {
            settings.Run.Headless = false;
        }
    }
}
