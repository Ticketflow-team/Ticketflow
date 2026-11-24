namespace TicketFlow.Guardian.Services;

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using TicketFlow.Guardian.Models;

public partial class OsvParser : IOsvParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public List<EnrichedFinding> ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var content = File.ReadAllText(filePath);
        return Parse(content);
    }

    public List<EnrichedFinding> Parse(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return [];
        }

        try
        {
            var report = JsonSerializer.Deserialize<OsvReport>(jsonContent, JsonOptions);
            if (report?.Results == null || report.Results.Count == 0)
            {
                return [];
            }

            var findings = new List<EnrichedFinding>();

            foreach (var result in report.Results)
            {
                if (result.Packages == null) continue;

                foreach (var pkgItem in result.Packages)
                {
                    var pkgName = pkgItem.Package?.Name ?? "Unknown";
                    var pkgVersion = pkgItem.Package?.Version ?? "Unknown";
                    var ecosystem = pkgItem.Package?.Ecosystem ?? "NuGet";

                    if (pkgItem.Vulnerabilities == null) continue;

                    foreach (var vuln in pkgItem.Vulnerabilities)
                    {
                        var id = vuln.Id ?? "UNKNOWN";
                        var aliases = vuln.Aliases ?? [];
                        var cvssScore = ExtractCvssScore(vuln.Severity);
                        var severity = DetermineSeverity(cvssScore, vuln.Severity);

                        findings.Add(new EnrichedFinding
                        {
                            Id = id,
                            Aliases = aliases,
                            Package = pkgName,
                            Version = pkgVersion,
                            Ecosystem = ecosystem,
                            Cvss = cvssScore,
                            Severity = severity,
                            Summary = vuln.Summary ?? string.Empty,
                            Details = vuln.Details ?? string.Empty,
                            Reachable = false,
                            Exposed = true,
                            Kev = false,
                            Epss = 0.0,
                            EpssPercentile = 0.0
                        });
                    }
                }
            }

            return findings;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[OsvParser] Warning: Failed to parse OSV JSON: {ex.Message}");
            return [];
        }
    }

    private static double? ExtractCvssScore(List<OsvSeverity>? severities)
    {
        if (severities == null || severities.Count == 0)
        {
            return null;
        }

        foreach (var sev in severities)
        {
            if (string.IsNullOrWhiteSpace(sev.Score)) continue;

            if (double.TryParse(sev.Score, CultureInfo.InvariantCulture, out var score))
            {
                return score;
            }

            var match = CvssScoreRegex().Match(sev.Score);
            if (match.Success && double.TryParse(match.Value, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static string DetermineSeverity(double? cvss, List<OsvSeverity>? severities)
    {
        if (cvss.HasValue)
        {
            if (cvss.Value >= 9.0) return "CRITICAL";
            if (cvss.Value >= 7.0) return "HIGH";
            if (cvss.Value >= 4.0) return "MODERATE";
            return "LOW";
        }

        if (severities != null && severities.Count > 0)
        {
            foreach (var s in severities)
            {
                var upper = (s.Type ?? string.Empty).ToUpperInvariant();
                if (upper.Contains("CRITICAL")) return "CRITICAL";
                if (upper.Contains("HIGH")) return "HIGH";
                if (upper.Contains("MODERATE") || upper.Contains("MEDIUM")) return "MODERATE";
                if (upper.Contains("LOW")) return "LOW";
            }
        }

        return "UNKNOWN";
    }

    [GeneratedRegex(@"\b\d{1,2}\.\d\b")]
    private static partial Regex CvssScoreRegex();
}
