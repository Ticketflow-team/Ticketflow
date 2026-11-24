namespace TicketFlow.Guardian.Services;

using TicketFlow.Guardian.Models;

public class EnrichmentPipeline : IEnrichmentPipeline
{
    private readonly IOsvParser _osvParser;
    private readonly ICisaKevClient _cisaKevClient;
    private readonly IEpssClient _epssClient;
    private readonly IReachabilityAnalyzer _reachabilityAnalyzer;

    public EnrichmentPipeline(
        IOsvParser? osvParser = null,
        ICisaKevClient? cisaKevClient = null,
        IEpssClient? epssClient = null,
        IReachabilityAnalyzer? reachabilityAnalyzer = null)
    {
        _osvParser = osvParser ?? new OsvParser();
        _cisaKevClient = cisaKevClient ?? new CisaKevClient();
        _epssClient = epssClient ?? new FirstEpssClient();
        _reachabilityAnalyzer = reachabilityAnalyzer ?? new ReachabilityAnalyzer();
    }

    public async Task<EnrichedReport> EnrichAsync(
        string osvJsonPath,
        string repoRoot,
        CancellationToken cancellationToken = default)
    {
        var findings = _osvParser.ParseFile(osvJsonPath);

        if (findings.Count == 0)
        {
            return new EnrichedReport
            {
                TotalFindings = 0,
                KevCount = 0,
                HighEpssCount = 0,
                ReachableCount = 0,
                Findings = []
            };
        }

        // Collect all CVE identifiers (from finding ID and aliases)
        var cveSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in findings)
        {
            if (f.Id.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase))
            {
                cveSet.Add(f.Id);
            }

            foreach (var alias in f.Aliases)
            {
                if (alias.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase))
                {
                    cveSet.Add(alias);
                }
            }
        }

        // Fetch CISA KEV and EPSS in parallel
        var kevTask = _cisaKevClient.GetKevCveIdsAsync(cancellationToken);
        var epssTask = _epssClient.GetEpssScoresAsync(cveSet, cancellationToken);

        await Task.WhenAll(kevTask, epssTask);

        var kevIds = await kevTask;
        var epssScores = await epssTask;

        // Enrich findings
        foreach (var f in findings)
        {
            // 1. KEV check
            var isKev = kevIds.Contains(f.Id) || f.Aliases.Any(a => kevIds.Contains(a));
            f.Kev = isKev;

            // 2. EPSS score lookup
            var primaryCve = f.Id.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase)
                ? f.Id
                : f.Aliases.FirstOrDefault(a => a.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(primaryCve) && epssScores.TryGetValue(primaryCve, out var scoreTuple))
            {
                f.Epss = scoreTuple.Epss;
                f.EpssPercentile = scoreTuple.Percentile;
            }

            // 3. Reachability and exposure analysis
            var (reachable, exposed) = _reachabilityAnalyzer.Analyze(repoRoot, f.Package);
            f.Reachable = reachable;
            f.Exposed = exposed;
        }

        return new EnrichedReport
        {
            TotalFindings = findings.Count,
            KevCount = findings.Count(f => f.Kev),
            HighEpssCount = findings.Count(f => f.Epss >= 0.1),
            ReachableCount = findings.Count(f => f.Reachable),
            Findings = findings
        };
    }
}
