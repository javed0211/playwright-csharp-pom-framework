using System.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightPom.Api.Client;
using PlaywrightPom.Core.Diagnostics;

namespace PlaywrightPom.NUnit;

public abstract class BaseApiTest
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static IPlaywright? _playwright;

    private readonly Stopwatch _timer = new();
    private TestStepLog _steps = TestStepLog.None;

    protected ApiClient Client { get; private set; } = null!;

    protected ApiClientFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task CreatePlaywrightAsync()
    {
        FrameworkRuntime.EnsureStarted("api");
        await Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            _playwright ??= await Playwright.CreateAsync().ConfigureAwait(false);
        }
        finally
        {
            Gate.Release();
        }
    }

    [SetUp]
    public async Task CreateClientAsync()
    {
        FrameworkRuntime.EnsureStarted("api");
        _timer.Restart();
        _steps = new TestStepLog(TestContext.CurrentContext.Test.Name);
        TestSession.Bind(TestContext.CurrentContext.Test.ID, _steps, Guid.NewGuid().ToString("N"));
        Factory = new ApiClientFactory(_playwright!, FrameworkRuntime.Settings.Api);
        Client = await Factory.CreateAsync().ConfigureAwait(false);
        _steps.Info($"API base {FrameworkRuntime.Settings.Api.BaseUrl}");
    }

    [TearDown]
    public async Task DisposeClientAsync()
    {
        _timer.Stop();
        if (Client is not null)
        {
            await Client.DisposeAsync().ConfigureAwait(false);
        }

        TestResultRecorder.Record(_steps, _timer.ElapsedMilliseconds, null, null, null);
        TestSession.Clear(TestContext.CurrentContext.Test.ID);
    }
}
