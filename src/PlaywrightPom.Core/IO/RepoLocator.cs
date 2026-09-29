using PlaywrightPom.Core.Configuration;

namespace PlaywrightPom.Core.IO;

public static class RepoLocator
{
    public static string? FindRepoRoot(string? start = null)
    {
        var dir = new DirectoryInfo(start ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "PlaywrightPom.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    public static string ArtifactRoot(TestSettings settings)
    {
        var configured = settings.Run.ArtifactRoot;
        if (Path.IsPathRooted(configured))
        {
            return configured;
        }

        var repo = FindRepoRoot() ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(repo, configured));
    }
}
