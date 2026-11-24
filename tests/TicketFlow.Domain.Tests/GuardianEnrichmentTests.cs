using TicketFlow.Guardian.Models;
using TicketFlow.Guardian.Services;
using Xunit;

namespace TicketFlow.Domain.Tests;

public class GuardianEnrichmentTests
{
    private const string SampleOsvJson = """
    {
      "results": [
        {
          "source": { "path": "src/TicketFlow.Api/packages.lock.json", "type": "lockfile" },
          "packages": [
            {
              "package": {
                "name": "Npgsql",
                "version": "4.0.10",
                "ecosystem": "NuGet"
              },
              "vulnerabilities": [
                {
                  "id": "GHSA-673j-qm5c-x38h",
                  "aliases": [ "CVE-2024-32655" ],
                  "summary": "Npgsql SQL Injection Vulnerability",
                  "details": "A vulnerability in Npgsql leads to SQL Injection.",
                  "severity": [ { "type": "CVSS_V3", "score": "8.5" } ]
                }
              ]
            }
          ]
        }
      ]
    }
    """;

    private const string SampleKevJson = """
    {
      "title": "CISA Catalog of Known Exploited Vulnerabilities",
      "count": 1,
      "vulnerabilities": [
        {
          "cveID": "CVE-2024-32655",
          "vendorProject": "Npgsql",
          "product": "Npgsql",
          "vulnerabilityName": "Npgsql Vulnerability"
        }
      ]
    }
    """;

    private const string SampleEpssJson = """
    {
      "status": "OK",
      "status-code": 200,
      "total": 1,
      "data": [
        {
          "cve": "CVE-2024-32655",
          "epss": "0.45000",
          "percentile": "0.92000",
          "date": "2026-11-24"
        }
      ]
    }
    """;

    [Fact]
    public void OsvParser_ParsesValidJson_ReturnsExpectedFindings()
    {
        var parser = new OsvParser();
        var findings = parser.Parse(SampleOsvJson);

        Assert.Single(findings);
        var finding = findings[0];
        Assert.Equal("GHSA-673j-qm5c-x38h", finding.Id);
        Assert.Contains("CVE-2024-32655", finding.Aliases);
        Assert.Equal("Npgsql", finding.Package);
        Assert.Equal("4.0.10", finding.Version);
        Assert.Equal(8.5, finding.Cvss);
        Assert.Equal("HIGH", finding.Severity);
    }

    [Fact]
    public void OsvParser_EmptyOrMalformedJson_ReturnsEmptyList()
    {
        var parser = new OsvParser();
        Assert.Empty(parser.Parse(string.Empty));
        Assert.Empty(parser.Parse("{ \"invalid\": true }"));
    }

    [Fact]
    public void CisaKevClient_ParseKevJson_ExtractsCaseInsensitiveSet()
    {
        var kevSet = CisaKevClient.ParseKevJson(SampleKevJson);

        Assert.Contains("CVE-2024-32655", kevSet);
        Assert.Contains("cve-2024-32655", kevSet);
        Assert.DoesNotContain("CVE-1999-0001", kevSet);
    }

    [Fact]
    public void FirstEpssClient_ParseEpssResponse_ExtractsScoresCorrectly()
    {
        var scores = FirstEpssClient.ParseEpssResponse(SampleEpssJson);

        Assert.True(scores.ContainsKey("CVE-2024-32655"));
        var (epss, percentile) = scores["CVE-2024-32655"];
        Assert.Equal(0.45, epss);
        Assert.Equal(0.92, percentile);
    }

    [Fact]
    public void ReachabilityAnalyzer_UnusedPackage_ReturnsNotReachable()
    {
        var analyzer = new ReachabilityAnalyzer();
        var tempDir = Directory.GetCurrentDirectory();

        var (reachable, _) = analyzer.Analyze(tempDir, "NonExistentVulnerablePackageXYZ");

        Assert.False(reachable);
    }

    [Fact]
    public async Task EnrichmentPipeline_CombinesSources_CorrectlyEnrichesReport()
    {
        var mockOsvParser = new MockOsvParser();
        var mockKevClient = new MockKevClient();
        var mockEpssClient = new MockEpssClient();
        var mockReachability = new MockReachabilityAnalyzer();

        var pipeline = new EnrichmentPipeline(mockOsvParser, mockKevClient, mockEpssClient, mockReachability);
        var report = await pipeline.EnrichAsync("dummy.json", "dummy-repo");

        Assert.Equal(1, report.TotalFindings);
        Assert.Equal(1, report.KevCount);
        Assert.Equal(1, report.HighEpssCount);
        Assert.Equal(1, report.ReachableCount);

        var finding = report.Findings[0];
        Assert.True(finding.Kev);
        Assert.Equal(0.45, finding.Epss);
        Assert.True(finding.Reachable);
    }

    private sealed class MockOsvParser : IOsvParser
    {
        public List<EnrichedFinding> Parse(string jsonContent) => ParseFile("dummy");

        public List<EnrichedFinding> ParseFile(string filePath) =>
        [
            new EnrichedFinding
            {
                Id = "GHSA-673j-qm5c-x38h",
                Aliases = ["CVE-2024-32655"],
                Package = "Npgsql",
                Version = "4.0.10",
                Ecosystem = "NuGet",
                Cvss = 8.5
            }
        ];
    }

    private sealed class MockKevClient : ICisaKevClient
    {
        public Task<HashSet<string>> GetKevCveIdsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CVE-2024-32655" });
    }

    private sealed class MockEpssClient : IEpssClient
    {
        public Task<Dictionary<string, (double Epss, double Percentile)>> GetEpssScoresAsync(
            IEnumerable<string> cveIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new Dictionary<string, (double Epss, double Percentile)>(StringComparer.OrdinalIgnoreCase)
            {
                ["CVE-2024-32655"] = (0.45, 0.92)
            });
    }

    private sealed class MockReachabilityAnalyzer : IReachabilityAnalyzer
    {
        public (bool Reachable, bool Exposed) Analyze(string repoRoot, string packageName) =>
            (Reachable: true, Exposed: true);
    }
}
