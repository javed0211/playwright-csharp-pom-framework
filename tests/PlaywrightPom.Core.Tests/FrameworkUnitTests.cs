using PlaywrightPom.Core;
using PlaywrightPom.Core.Assertions;
using PlaywrightPom.Core.Configuration;
using PlaywrightPom.Core.IO;
using PlaywrightPom.Core.Logging;
using PlaywrightPom.Core.Resilience;
using PlaywrightPom.Reporting;
using Shouldly;

namespace PlaywrightPom.Core.Tests;

[TestFixture]
[Category(Traits.Unit)]
public sealed class ConfigurationTests
{
    [Test]
    public void ShippedProfiles_BindAndOverrideTimeouts()
    {
        var root = RepoLocator.FindRepoRoot();
        root.ShouldNotBeNull();
        var config = Path.Combine(root, "config");

        var development = TestConfiguration.Load(config, "Development");
        var ci = TestConfiguration.Load(config, "CI");

        development.EnvironmentName.ShouldBe("Development");
        development.Web.BaseUrl.ShouldBe("https://www.saucedemo.com");
        development.Api.BaseUrl.ShouldBe("https://dummyjson.com");
        development.Run.TimeoutMs.ShouldBe(30_000);
        development.Run.LogLevel.ShouldBe("Debug");

        ci.EnvironmentName.ShouldBe("CI");
        ci.Run.TimeoutMs.ShouldBe(45_000);
        ci.Run.ExpectTimeoutMs.ShouldBe(15_000);
        ci.Api.TimeoutSeconds.ShouldBe(40);
        ci.EnsureRunnable();
    }

    [Test]
    public void InMemoryOverride_ReplacesWebBaseUrl()
    {
        var root = RepoLocator.FindRepoRoot();
        root.ShouldNotBeNull();
        var settings = TestConfiguration.Load(
            Path.Combine(root, "config"),
            "Development",
            new Dictionary<string, string?>
            {
                ["Web:BaseUrl"] = "https://example.test",
            });

        settings.Web.BaseUrl.ShouldBe("https://example.test");
        settings.Api.BaseUrl.ShouldBe("https://dummyjson.com");
    }
}

[TestFixture]
[NonParallelizable]
[Category(Traits.Unit)]
public sealed class EnvironmentOverrideTests
{
    [Test]
    public void EnvironmentVariable_OverridesApiBaseUrl()
    {
        var root = RepoLocator.FindRepoRoot();
        root.ShouldNotBeNull();
        const string key = "NEXUS_Api__BaseUrl";
        var previous = Environment.GetEnvironmentVariable(key);
        try
        {
            Environment.SetEnvironmentVariable(key, "https://api.example.test");
            var settings = TestConfiguration.Load(Path.Combine(root, "config"), "Development");
            settings.Api.BaseUrl.ShouldBe("https://api.example.test");
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, previous);
        }
    }
}

[TestFixture]
[Category(Traits.Unit)]
public sealed class SecretRedactorTests
{
    [Test]
    public void Redact_MasksBearerTokensJsonSecretsAndQueryValues()
    {
        const string input = "Authorization: Bearer abc.def-123 password=hunter2 {\"accessToken\":\"jwt-value\",\"password\":\"secret_sauce\"}";
        var redacted = SecretRedactor.Redact(input);

        redacted.ShouldNotContain("abc.def-123");
        redacted.ShouldNotContain("hunter2");
        redacted.ShouldNotContain("jwt-value");
        redacted.ShouldNotContain("secret_sauce");
        redacted.ShouldContain("Bearer [redacted]");
        redacted.ShouldContain("[redacted]");
    }
}

[TestFixture]
[Category(Traits.Unit)]
public sealed class SoftAssertTests
{
    [Test]
    public void AssertAll_CollectsEveryFailure()
    {
        var softly = new SoftAssert();
        softly.Check(false, "first missing");
        softly.Check(true, "ok");
        softly.Check(false, "second missing");

        var error = Should.Throw<AssertionAggregateException>(() => softly.AssertAll());
        error.Failures.Count.ShouldBe(2);
        error.Message.ShouldContain("first missing");
        error.Message.ShouldContain("second missing");
    }
}

[TestFixture]
[Category(Traits.Unit)]
public sealed class TransientRetryTests
{
    [Test]
    public async Task Create_RetriesTransientFailuresThenSucceeds()
    {
        var attempts = 0;
        var pipeline = TransientRetry.Create(2, TimeSpan.FromMilliseconds(1));

        await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new TransientRequestException("not yet");
            }

            return ValueTask.CompletedTask;
        });

        attempts.ShouldBe(3);
    }
}

[TestFixture]
[Category(Traits.Unit)]
public sealed class HtmlReportTests
{
    [Test]
    public void Write_EscapesMarkupAndLinksFailedCases()
    {
        var directory = Directory.CreateTempSubdirectory("pom-report");
        var shot = Path.Combine(directory.FullName, "shot.png");
        File.WriteAllBytes(shot, [1, 2, 3]);
        var reportPath = Path.Combine(directory.FullName, "reports", "unit-report.html");
        var report = new TestRunReport
        {
            Suite = "unit",
            Environment = "Development",
            Browser = "chromium",
            RunId = "abc",
            StartedUtc = "2026-09-29T00:00:00Z",
            WebBaseUrl = "https://www.saucedemo.com",
            ApiBaseUrl = "https://dummyjson.com",
            Cases =
            [
                new TestCaseResult
                {
                    Name = "fails <script>",
                    FullName = "sample.fails",
                    Outcome = "Failed",
                    CaseId = "UNIT-1",
                    Categories = ["Unit"],
                    DurationMs = 12,
                    Steps = ["STEP open"],
                    Error = "<script>alert(1)</script>",
                    ScreenshotPath = shot,
                },
            ],
        };

        HtmlReportWriter.Write(report, reportPath);
        var html = File.ReadAllText(reportPath);

        html.ShouldContain("fails &lt;script&gt;");
        html.ShouldContain("&lt;script&gt;alert(1)&lt;/script&gt;");
        html.ShouldNotContain("<script>alert");
        html.ShouldContain("shot.png");
        html.ShouldContain("Failed");
    }
}
