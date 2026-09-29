using System.Text.RegularExpressions;

namespace PlaywrightPom.Core.Logging;

public static partial class SecretRedactor
{
    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var redacted = BearerPattern().Replace(value, "Bearer [redacted]");
        redacted = JsonSecretPattern().Replace(redacted, "$1[redacted]");
        redacted = QuerySecretPattern().Replace(redacted, "$1[redacted]");
        return redacted;
    }

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BearerPattern();

    [GeneratedRegex(@"((?:password|accessToken|refreshToken|secret|authorization)""?\s*:\s*"")[^""]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JsonSecretPattern();

    [GeneratedRegex(@"((?:password|token|secret|access_token)=)[^&\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex QuerySecretPattern();
}
