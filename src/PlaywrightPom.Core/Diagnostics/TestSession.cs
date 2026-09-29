using System.Collections.Concurrent;

namespace PlaywrightPom.Core.Diagnostics;

public static class TestSession
{
    private static readonly AsyncLocal<TestStepLog?> CurrentLog = new();
    private static readonly AsyncLocal<string?> CurrentCorrelation = new();
    private static readonly ConcurrentDictionary<string, SessionState> Sessions = new();
    private static Func<string?>? _keyResolver;

    public static TestStepLog Steps
    {
        get
        {
            var key = _keyResolver?.Invoke();
            if (!string.IsNullOrEmpty(key) && Sessions.TryGetValue(key, out var state))
            {
                return state.Log;
            }

            return CurrentLog.Value ?? TestStepLog.None;
        }
    }

    public static string CorrelationId
    {
        get
        {
            var key = _keyResolver?.Invoke();
            if (!string.IsNullOrEmpty(key) && Sessions.TryGetValue(key, out var state))
            {
                return state.CorrelationId;
            }

            return CurrentCorrelation.Value ?? "unset";
        }
    }

    public static void UseKeyResolver(Func<string?> resolver) => _keyResolver = resolver;

    public static void Bind(string key, TestStepLog log, string correlationId)
    {
        Sessions[key] = new SessionState(log, correlationId);
        CurrentLog.Value = log;
        CurrentCorrelation.Value = correlationId;
    }

    public static void Clear(string key)
    {
        Sessions.TryRemove(key, out _);
        CurrentLog.Value = null;
        CurrentCorrelation.Value = null;
    }

    private sealed record SessionState(TestStepLog Log, string CorrelationId);
}
