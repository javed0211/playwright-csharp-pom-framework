using Microsoft.Extensions.Configuration;

namespace PlaywrightPom.Core.Configuration;

public static class TestConfiguration
{
    private static readonly object Gate = new();
    private static TestSettings? _current;

    public static TestSettings Current
    {
        get
        {
            lock (Gate)
            {
                return _current ??= Load();
            }
        }
    }

    public static void Use(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        lock (Gate)
        {
            _current = settings;
        }
    }

    public static void Reset()
    {
        lock (Gate)
        {
            _current = null;
        }
    }

    public static TestSettings Load(
        string? configDirectory = null,
        string? environment = null,
        IDictionary<string, string?>? overrides = null)
    {
        var directory = ResolveConfigDirectory(configDirectory);
        var environmentName = environment
            ?? Environment.GetEnvironmentVariable("NEXUS_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "NEXUS_")
            .AddInMemoryCollection(overrides)
            .Build();

        var settings = new TestSettings();
        configuration.Bind(settings);
        settings.EnvironmentName = environmentName;
        return settings;
    }

    public static string ResolveConfigDirectory(string? explicitDirectory = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitDirectory))
        {
            EnsureConfigFile(explicitDirectory);
            return explicitDirectory;
        }

        var fromEnv = Environment.GetEnvironmentVariable("NEXUS_CONFIG_DIR");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(Path.Combine(fromEnv, "appsettings.json")))
        {
            return fromEnv;
        }

        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var found = FindConfigDirectory(start);
            if (found is not null)
            {
                return found;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not find config/appsettings.json. Set NEXUS_CONFIG_DIR or run tests from the repository.");
    }

    private static string? FindConfigDirectory(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            var nested = Path.Combine(dir.FullName, "config", "appsettings.json");
            if (File.Exists(nested))
            {
                return Path.Combine(dir.FullName, "config");
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static void EnsureConfigFile(string directory)
    {
        if (!File.Exists(Path.Combine(directory, "appsettings.json")))
        {
            throw new DirectoryNotFoundException($"appsettings.json was not found in '{directory}'.");
        }
    }
}
