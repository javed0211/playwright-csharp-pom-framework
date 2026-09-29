using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using PlaywrightPom.Core.Diagnostics;
using PlaywrightPom.Reporting;

namespace PlaywrightPom.NUnit;

public static class TestResultRecorder
{
    public static void Record(TestStepLog steps, long durationMs, string? screenshotPath, string? tracePath, IEnumerable<string>? notes)
    {
        var context = TestContext.CurrentContext;
        var status = context.Result.Outcome.Status;
        var outcome = status.ToString();
        var error = status == TestStatus.Failed ? context.Result.Message : null;
        if (status == TestStatus.Failed && notes is not null)
        {
            var extra = string.Join(Environment.NewLine, notes.Where(note => !string.IsNullOrWhiteSpace(note)));
            if (!string.IsNullOrWhiteSpace(extra))
            {
                error = string.IsNullOrWhiteSpace(error) ? extra : error + Environment.NewLine + extra;
            }
        }

        Attach(screenshotPath);
        Attach(tracePath);

        ReportCollector.Instance.Add(new PlaywrightPom.Reporting.TestCaseResult
        {
            Name = context.Test.Name,
            FullName = context.Test.FullName,
            Outcome = outcome,
            CaseId = FirstProperty("CaseId"),
            Categories = ReadCategories(),
            DurationMs = durationMs,
            Steps = steps.Snapshot(),
            Error = error,
            StackTrace = status == TestStatus.Failed ? context.Result.StackTrace : null,
            ScreenshotPath = screenshotPath,
            TracePath = tracePath,
        });
    }

    private static void Attach(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            TestContext.AddTestAttachment(path);
        }
    }

    private static string? FirstProperty(string name)
    {
        foreach (var value in TestContext.CurrentContext.Test.Properties[name])
        {
            return value?.ToString();
        }

        return null;
    }

    private static IReadOnlyList<string> ReadCategories()
    {
        var values = new List<string>();
        ITest? test = TestExecutionContext.CurrentContext.CurrentTest;
        while (test is not null)
        {
            foreach (var value in test.Properties["Category"])
            {
                var text = value?.ToString();
                if (!string.IsNullOrWhiteSpace(text) && !values.Contains(text))
                {
                    values.Add(text);
                }
            }

            test = test.Parent;
        }

        return values;
    }
}
