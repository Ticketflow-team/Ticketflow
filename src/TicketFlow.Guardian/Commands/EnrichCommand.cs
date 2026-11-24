namespace TicketFlow.Guardian.Commands;

using System.Text.Json;
using TicketFlow.Guardian.Models;
using TicketFlow.Guardian.Services;

public class EnrichCommand
{
    private static readonly JsonSerializerOptions JsonIndentOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IEnrichmentPipeline _pipeline;

    public EnrichCommand(IEnrichmentPipeline? pipeline = null)
    {
        _pipeline = pipeline ?? new EnrichmentPipeline();
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        string inputPath = "osv-report.json";
        string outputPath = "findings.enriched.json";
        string repoRoot = Directory.GetCurrentDirectory();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals("--output", StringComparison.OrdinalIgnoreCase) || arg.Equals("-o", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }
            else if (arg.Equals("--repo-root", StringComparison.OrdinalIgnoreCase) || arg.Equals("-r", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    repoRoot = args[++i];
                }
            }
            else if (!arg.StartsWith('-') && !arg.Equals("enrich", StringComparison.OrdinalIgnoreCase))
            {
                inputPath = arg;
            }
        }

        Console.WriteLine($"[TicketFlow.Guardian] Starting finding enrichment (P9)");
        Console.WriteLine($"  Input OSV scan  : {inputPath}");
        Console.WriteLine($"  Target output   : {outputPath}");
        Console.WriteLine($"  Repository root : {repoRoot}");

        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"[TicketFlow.Guardian] Notice: Input file '{inputPath}' does not exist or has 0 findings. Creating empty enriched report.");
            var emptyReport = new EnrichedReport
            {
                TotalFindings = 0,
                KevCount = 0,
                HighEpssCount = 0,
                ReachableCount = 0,
                Findings = []
            };

            var emptyJson = JsonSerializer.Serialize(emptyReport, JsonIndentOptions);
            await File.WriteAllTextAsync(outputPath, emptyJson);
            Console.WriteLine($"[TicketFlow.Guardian] Successfully generated: {outputPath} (0 findings)");
            return 0;
        }

        var report = await _pipeline.EnrichAsync(inputPath, repoRoot);

        var outputJson = JsonSerializer.Serialize(report, JsonIndentOptions);
        await File.WriteAllTextAsync(outputPath, outputJson);

        Console.WriteLine($"\n[TicketFlow.Guardian] Enrichment completed successfully!");
        Console.WriteLine($"  Total Findings  : {report.TotalFindings}");
        Console.WriteLine($"  In CISA KEV     : {report.KevCount}");
        Console.WriteLine($"  EPSS >= 0.1     : {report.HighEpssCount}");
        Console.WriteLine($"  Reachable Code  : {report.ReachableCount}");
        Console.WriteLine($"  Output saved to : {outputPath}");

        if (report.KevCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[WARNING] {report.KevCount} finding(s) detected in CISA Known Exploited Vulnerabilities catalog!");
            Console.ResetColor();
        }

        return 0;
    }
}
