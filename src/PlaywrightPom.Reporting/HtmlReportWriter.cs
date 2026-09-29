using System.Net;
using System.Text;

namespace PlaywrightPom.Reporting;

public static class HtmlReportWriter
{
    public static void Write(TestRunReport report, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var passed = report.Cases.Count(c => c.Outcome == "Passed");
        var failed = report.Cases.Count(c => c.Outcome == "Failed");
        var other = report.Cases.Count - passed - failed;
        var duration = report.Cases.Sum(c => c.DurationMs);
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"utf-8\" />");
        builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        builder.Append("<title>").Append(E(report.Suite)).AppendLine(" report</title>");
        builder.AppendLine("<style>");
        builder.AppendLine(Css);
        builder.AppendLine("</style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("<header class=\"top\">");
        builder.AppendLine("<p class=\"eyebrow\">Playwright POM</p>");
        builder.Append("<h1>").Append(E(report.Suite)).AppendLine(" run</h1>");
        builder.Append("<p class=\"meta\">");
        builder.Append(E(report.Environment));
        builder.Append(" · ");
        builder.Append(E(report.Browser));
        builder.Append(" · ");
        builder.Append(E(report.RunId));
        builder.Append(" · ");
        builder.Append(duration).Append(" ms");
        builder.AppendLine("</p>");
        builder.Append("<p class=\"meta\">Web ").Append(E(report.WebBaseUrl));
        builder.Append(" · API ").Append(E(report.ApiBaseUrl)).AppendLine("</p>");
        builder.AppendLine("</header>");
        builder.AppendLine("<section class=\"stats\">");
        Stat(builder, "Passed", passed, "pass");
        Stat(builder, "Failed", failed, "fail");
        Stat(builder, "Other", other, "other");
        Stat(builder, "Total", report.Cases.Count, "total");
        builder.AppendLine("</section>");
        builder.AppendLine("<main>");

        foreach (var test in report.Cases)
        {
            var failedCase = test.Outcome == "Failed";
            builder.Append("<article class=\"case ").Append(failedCase ? "fail" : "pass").AppendLine("\">");
            builder.AppendLine("<header>");
            builder.Append("<h2>").Append(E(test.Name)).AppendLine("</h2>");
            builder.Append("<span class=\"pill\">").Append(E(test.Outcome)).AppendLine("</span>");
            builder.AppendLine("</header>");
            builder.Append("<p class=\"sub\">");
            if (!string.IsNullOrWhiteSpace(test.CaseId))
            {
                builder.Append(E(test.CaseId)).Append(" · ");
            }

            builder.Append(E(string.Join(", ", test.Categories)));
            builder.Append(" · ").Append(test.DurationMs).AppendLine(" ms</p>");
            if (!string.IsNullOrWhiteSpace(test.Error))
            {
                builder.Append("<pre class=\"error\">").Append(E(test.Error)).AppendLine("</pre>");
            }

            if (!string.IsNullOrWhiteSpace(test.StackTrace))
            {
                builder.Append("<details><summary>Stack</summary><pre>").Append(E(test.StackTrace)).AppendLine("</pre></details>");
            }

            if (test.Steps.Count > 0)
            {
                builder.AppendLine(failedCase ? "<ol class=\"steps\">" : "<details><summary>Steps</summary><ol class=\"steps\">");
                foreach (var step in test.Steps)
                {
                    builder.Append("<li>").Append(E(step)).AppendLine("</li>");
                }

                builder.AppendLine(failedCase ? "</ol>" : "</ol></details>");
            }

            var shot = Relative(path, test.ScreenshotPath);
            var trace = Relative(path, test.TracePath);
            if (shot is not null || trace is not null)
            {
                builder.Append("<p class=\"links\">");
                if (shot is not null)
                {
                    builder.Append("<a href=\"").Append(E(shot)).AppendLine("\">Screenshot</a>");
                }

                if (trace is not null)
                {
                    builder.Append("<a href=\"").Append(E(trace)).AppendLine("\">Trace</a>");
                }

                builder.AppendLine("</p>");
            }

            builder.AppendLine("</article>");
        }

        if (report.Cases.Count == 0)
        {
            builder.AppendLine("<p class=\"empty\">No tests were recorded.</p>");
        }

        builder.AppendLine("</main>");
        builder.Append("<footer>Started ").Append(E(report.StartedUtc));
        builder.Append(" · Finished ").Append(E(report.FinishedUtc)).AppendLine("</footer>");
        builder.AppendLine("</body></html>");
        File.WriteAllText(path, builder.ToString());
    }

    private static void Stat(StringBuilder builder, string label, int value, string kind)
    {
        builder.Append("<article class=\"stat ").Append(kind).Append("\"><span>").Append(E(label));
        builder.Append("</span><strong>").Append(value).AppendLine("</strong></article>");
    }

    private static string? Relative(string reportPath, string? target)
    {
        if (string.IsNullOrWhiteSpace(target) || !File.Exists(target))
        {
            return null;
        }

        var reportDir = Path.GetDirectoryName(reportPath) ?? Directory.GetCurrentDirectory();
        return Path.GetRelativePath(reportDir, target).Replace('\\', '/');
    }

    private static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private const string Css = """
        :root { color-scheme: light; --ink:#1c1915; --muted:#5c564e; --line:#e4ddd2; --paper:#f7f4ef; --card:#fffdf9; --pass:#1f7a4d; --fail:#a33b32; --accent:#8a5a2b; }
        * { box-sizing: border-box; }
        body { margin:0; font:16px/1.5 "Iowan Old Style", Palatino, "Palatino Linotype", Georgia, serif; color:var(--ink); background:var(--paper); }
        .top, main, footer { width:min(980px, calc(100% - 32px)); margin:0 auto; }
        .top { padding:40px 0 8px; }
        .eyebrow { letter-spacing:.16em; text-transform:uppercase; font:600 12px/1 ui-sans-serif, system-ui, sans-serif; color:var(--accent); margin:0 0 8px; }
        h1 { font-size:40px; line-height:1.1; margin:0 0 8px; font-weight:560; }
        .meta { margin:0; color:var(--muted); font:14px/1.4 ui-sans-serif, system-ui, sans-serif; }
        .stats { width:min(980px, calc(100% - 32px)); margin:24px auto; display:grid; grid-template-columns:repeat(4, 1fr); gap:12px; }
        .stat { background:var(--card); border:1px solid var(--line); border-radius:14px; padding:14px 16px; }
        .stat span { display:block; font:12px/1 ui-sans-serif, system-ui, sans-serif; color:var(--muted); text-transform:uppercase; letter-spacing:.08em; }
        .stat strong { display:block; margin-top:8px; font-size:28px; }
        .stat.pass strong { color:var(--pass); }
        .stat.fail strong { color:var(--fail); }
        .case { background:var(--card); border:1px solid var(--line); border-radius:16px; padding:16px 18px; margin:0 0 12px; }
        .case.fail { border-color:#e7c7c3; box-shadow:inset 4px 0 0 var(--fail); }
        .case header { display:flex; justify-content:space-between; gap:12px; align-items:baseline; }
        h2 { font-size:18px; margin:0; }
        .pill { font:600 12px/1 ui-sans-serif, system-ui, sans-serif; border-radius:999px; padding:6px 8px; background:#efeae3; }
        .case.fail .pill { background:#f8e8e6; color:var(--fail); }
        .case.pass .pill { background:#e5f4ec; color:var(--pass); }
        .sub { color:var(--muted); font:13px/1.4 ui-sans-serif, system-ui, sans-serif; margin:6px 0 0; }
        pre { white-space:pre-wrap; background:#1c1915; color:#f7f4ef; border-radius:10px; padding:12px; overflow:auto; font:13px/1.45 ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; }
        .steps { margin:8px 0 0; padding-left:18px; }
        .links a { margin-right:12px; color:var(--accent); }
        .empty { color:var(--muted); }
        footer { color:var(--muted); font:12px/1.4 ui-sans-serif, system-ui, sans-serif; padding:12px 0 40px; }
        @media (max-width:720px) { .stats { grid-template-columns:1fr 1fr; } h1 { font-size:32px; } }
        """;
}
