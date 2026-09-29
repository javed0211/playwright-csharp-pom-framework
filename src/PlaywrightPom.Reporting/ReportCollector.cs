using PlaywrightPom.Core.Configuration;

namespace PlaywrightPom.Reporting;

public sealed class ReportCollector
{
    public static ReportCollector Instance { get; } = new();

    private readonly List<TestCaseResult> _cases = [];
    private readonly object _gate = new();
    private TestRunReport? _report;
    private bool _flushed;

    public void Begin(string suite, TestSettings settings, string runId)
    {
        lock (_gate)
        {
            _cases.Clear();
            _flushed = false;
            _report = new TestRunReport
            {
                Suite = suite,
                Environment = settings.EnvironmentName,
                Browser = settings.Run.Browser,
                RunId = runId,
                StartedUtc = DateTime.UtcNow.ToString("o"),
                WebBaseUrl = settings.Web.BaseUrl,
                ApiBaseUrl = settings.Api.BaseUrl,
            };
        }
    }

    public void Add(TestCaseResult result)
    {
        lock (_gate)
        {
            _cases.Add(result);
        }
    }

    public string? Flush(TestSettings settings, string suite)
    {
        List<TestCaseResult> cases;
        TestRunReport report;
        lock (_gate)
        {
            if (_flushed || _report is null)
            {
                return null;
            }

            _flushed = true;
            cases = _cases.ToList();
            report = _report;
            report.FinishedUtc = DateTime.UtcNow.ToString("o");
            report.Cases.AddRange(cases.OrderBy(c => c.Outcome == "Failed" ? 0 : 1).ThenBy(c => c.FullName));
        }

        var directory = Core.Artifacts.ArtifactLayout.Reports(settings);
        var htmlPath = Path.Combine(directory, $"{suite}-report.html");
        var jsonPath = Path.Combine(directory, $"{suite}-report.json");
        HtmlReportWriter.Write(report, htmlPath);
        JsonReportWriter.Write(report, jsonPath);
        return htmlPath;
    }
}
