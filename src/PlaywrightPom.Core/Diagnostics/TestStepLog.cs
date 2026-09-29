using PlaywrightPom.Core.Logging;

namespace PlaywrightPom.Core.Diagnostics;

public sealed class TestStepLog
{
    public static TestStepLog None { get; } = new("none");

    private readonly List<string> _lines = [];
    private readonly object _gate = new();

    public TestStepLog(string testName) => TestName = testName;

    public string TestName { get; }

    public void Info(string message)
    {
        var line = SecretRedactor.Redact(message);
        lock (_gate)
        {
            _lines.Add(line);
        }

        FrameworkLog.Information($"[{TestName}] {line}");
    }

    public void Error(string message)
    {
        var line = "ERROR " + SecretRedactor.Redact(message);
        lock (_gate)
        {
            _lines.Add(line);
        }

        FrameworkLog.Error($"[{TestName}] {line}");
    }

    public async Task StepAsync(string name, Func<Task> action)
    {
        Info($"STEP {name}");
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Error($"{name} failed: {ex.Message}");
            throw;
        }
    }

    public IReadOnlyList<string> Snapshot()
    {
        lock (_gate)
        {
            return _lines.ToArray();
        }
    }
}
