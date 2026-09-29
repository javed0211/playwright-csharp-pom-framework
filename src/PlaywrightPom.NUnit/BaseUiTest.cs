using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using PlaywrightPom.Core.Artifacts;
using PlaywrightPom.Core.Configuration;
using PlaywrightPom.Core.Diagnostics;

namespace PlaywrightPom.NUnit;

public abstract class BaseUiTest : PageTest
{
    private readonly Stopwatch _timer = new();
    private readonly UiDiagnostics _diagnostics = new();
    private TestStepLog _steps = TestStepLog.None;
    private bool _configured;

    protected TestSettings Settings => FrameworkRuntime.Settings;

    public override Task<BrowserTypeLaunchOptions?> LaunchOptionsAsync()
    {
        FrameworkRuntime.EnsureStarted("web");
        var run = FrameworkRuntime.Settings.Run;
        return Task.FromResult<BrowserTypeLaunchOptions?>(new BrowserTypeLaunchOptions
        {
            Headless = run.Headless,
            SlowMo = run.SlowMoMs,
            Channel = string.IsNullOrWhiteSpace(run.Channel) ? null : run.Channel,
            Args = ["--disable-dev-shm-usage"],
        });
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        FrameworkRuntime.EnsureStarted("web");
        var settings = FrameworkRuntime.Settings;
        var options = new BrowserNewContextOptions
        {
            BaseURL = settings.Web.BaseUrl,
            IgnoreHTTPSErrors = true,
            Locale = "en-US",
            TimezoneId = "UTC",
            ViewportSize = new ViewportSize
            {
                Width = settings.Run.ViewportWidth,
                Height = settings.Run.ViewportHeight,
            },
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["X-Test-Run"] = FrameworkRuntime.RunId,
            },
        };

        if (settings.Run.RecordVideo)
        {
            options.RecordVideoDir = ArtifactLayout.Videos(settings);
            options.RecordVideoSize = new RecordVideoSize
            {
                Width = settings.Run.ViewportWidth,
                Height = settings.Run.ViewportHeight,
            };
        }

        return options;
    }

    [SetUp]
    public async Task FrameworkSetUp()
    {
        FrameworkRuntime.EnsureStarted("web");
        _timer.Restart();
        var testName = TestContext.CurrentContext.Test.Name;
        _steps = new TestStepLog(testName);
        var correlation = Guid.NewGuid().ToString("N");
        TestSession.Bind(TestContext.CurrentContext.Test.ID, _steps, correlation);
        Playwright.Selectors.SetTestIdAttribute(Settings.Web.TestIdAttribute);
        SetDefaultExpectTimeout(Settings.Run.ExpectTimeoutMs);
        Page.SetDefaultTimeout(Settings.Run.TimeoutMs);
        Page.SetDefaultNavigationTimeout(Settings.Run.NavigationTimeoutMs);
        _diagnostics.Attach(Page);
        if (!Settings.Run.Trace.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            await _diagnostics.StartTraceAsync(Context, TestContext.CurrentContext.Test.FullName).ConfigureAwait(false);
        }

        _configured = true;
        _steps.Info($"Correlation {correlation}");
    }

    [TearDown]
    public async Task FrameworkTearDown()
    {
        _timer.Stop();
        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        var settings = FrameworkRuntime.Settings;
        string? screenshot = null;
        string? trace = null;

        if (_configured)
        {
            var token = ArtifactLayout.SafeFileName(TestContext.CurrentContext.Test.Name);
            var stamp = $"{token}-{Guid.NewGuid().ToString("N")[..8]}";
            var saveShot = failed && settings.Run.CaptureScreenshotOnFailure;
            var saveTrace = settings.Run.Trace.Equals("on", StringComparison.OrdinalIgnoreCase)
                || (failed && settings.Run.Trace.Equals("on-failure", StringComparison.OrdinalIgnoreCase));
            var artifacts = await _diagnostics.StopAsync(
                Page,
                Context,
                saveShot,
                saveTrace,
                Path.Combine(ArtifactLayout.Screenshots(settings), stamp + ".png"),
                Path.Combine(ArtifactLayout.Traces(settings), stamp + ".zip")).ConfigureAwait(false);
            screenshot = artifacts.Screenshot;
            trace = artifacts.Trace;
        }

        TestResultRecorder.Record(_steps, _timer.ElapsedMilliseconds, screenshot, trace, _diagnostics.BrowserLogs);
        TestSession.Clear(TestContext.CurrentContext.Test.ID);
    }
}
