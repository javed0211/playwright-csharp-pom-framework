using System.Text.RegularExpressions;
using PlaywrightPom.Core.Configuration;

namespace PlaywrightPom.Core.Artifacts;

public static class ArtifactLayout
{
    public static string Root(TestSettings settings) => Ensure(settings.Run.ArtifactRoot);

    public static string Logs(TestSettings settings) => Ensure(Path.Combine(Root(settings), "logs"));

    public static string Reports(TestSettings settings) => Ensure(Path.Combine(Root(settings), "reports"));

    public static string Screenshots(TestSettings settings) => Ensure(Path.Combine(Root(settings), "screenshots"));

    public static string Traces(TestSettings settings) => Ensure(Path.Combine(Root(settings), "traces"));

    public static string Videos(TestSettings settings) => Ensure(Path.Combine(Root(settings), "videos"));

    public static string SafeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "artifact";
        }

        var invalidCharacters = $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]";
        var cleaned = Regex.Replace(value, invalidCharacters, "-");
        cleaned = Regex.Replace(cleaned, "\\s+", "-");
        cleaned = cleaned.Trim('-', '.');
        return string.IsNullOrWhiteSpace(cleaned) ? "artifact" : cleaned;
    }

    private static string Ensure(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
