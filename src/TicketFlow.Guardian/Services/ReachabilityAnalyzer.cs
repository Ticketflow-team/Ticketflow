namespace TicketFlow.Guardian.Services;

using System.Text.RegularExpressions;

public partial class ReachabilityAnalyzer : IReachabilityAnalyzer
{
    public (bool Reachable, bool Exposed) Analyze(string repoRoot, string packageName)
    {
        if (string.IsNullOrWhiteSpace(repoRoot) || !Directory.Exists(repoRoot))
        {
            return (Reachable: true, Exposed: true);
        }

        if (string.IsNullOrWhiteSpace(packageName))
        {
            return (Reachable: false, Exposed: false);
        }

        var srcDir = Path.Combine(repoRoot, "src");
        if (!Directory.Exists(srcDir))
        {
            srcDir = repoRoot;
        }

        var (referencedInSrc, exposedInApi) = CheckProjectReferences(repoRoot, srcDir, packageName);
        var (usedInCode, usedInApi) = CheckCodeUsages(srcDir, packageName);

        var reachable = referencedInSrc || usedInCode;
        var exposed = exposedInApi || usedInApi;

        return (Reachable: reachable, Exposed: exposed);
    }

    private static (bool InSrc, bool InApi) CheckProjectReferences(string repoRoot, string srcDir, string packageName)
    {
        var referencedInSrc = false;
        var exposedInApi = false;

        var csprojFiles = Directory.GetFiles(repoRoot, "*.csproj", SearchOption.AllDirectories);
        var escapedName = Regex.Escape(packageName);
        var packageRefRegex = new Regex($@"<PackageReference\s+[^>]*Include\s*=\s*""{escapedName}""", RegexOptions.IgnoreCase);

        foreach (var file in csprojFiles)
        {
            var content = File.ReadAllText(file);
            if (!packageRefRegex.IsMatch(content)) continue;

            var normalizedPath = file.Replace('\\', '/');
            if (normalizedPath.Contains("/src/"))
            {
                referencedInSrc = true;
                if (normalizedPath.Contains("TicketFlow.Api") || normalizedPath.Contains("/Api/"))
                {
                    exposedInApi = true;
                }
            }
        }

        return (referencedInSrc, exposedInApi);
    }

    private static (bool InCode, bool InApi) CheckCodeUsages(string srcDir, string packageName)
    {
        var usedInCode = false;
        var usedInApi = false;

        var csFiles = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories);
        var shortName = packageName.Split('.').Last();
        var usingPattern = $@"\busing\s+.*{Regex.Escape(shortName)}";
        var usingRegex = new Regex(usingPattern, RegexOptions.IgnoreCase);

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (usingRegex.IsMatch(content))
            {
                usedInCode = true;
                var normalizedPath = file.Replace('\\', '/');
                if (normalizedPath.Contains("TicketFlow.Api") || normalizedPath.Contains("/Api/"))
                {
                    usedInApi = true;
                }
            }
        }

        return (usedInCode, usedInApi);
    }
}
