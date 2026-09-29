namespace PlaywrightPom.Reporting;

public sealed class TestCaseResult
{
    public string Name { get; init; } = "";

    public string FullName { get; init; } = "";

    public string Outcome { get; init; } = "";

    public string? CaseId { get; init; }

    public IReadOnlyList<string> Categories { get; init; } = [];

    public long DurationMs { get; init; }

    public IReadOnlyList<string> Steps { get; init; } = [];

    public string? Error { get; init; }

    public string? StackTrace { get; init; }

    public string? ScreenshotPath { get; init; }

    public string? TracePath { get; init; }
}

public sealed class TestRunReport
{
    public string Suite { get; init; } = "";

    public string Environment { get; init; } = "";

    public string Browser { get; init; } = "";

    public string RunId { get; init; } = "";

    public string StartedUtc { get; init; } = "";

    public string FinishedUtc { get; set; } = "";

    public string WebBaseUrl { get; init; } = "";

    public string ApiBaseUrl { get; init; } = "";

    public List<TestCaseResult> Cases { get; init; } = [];
}
