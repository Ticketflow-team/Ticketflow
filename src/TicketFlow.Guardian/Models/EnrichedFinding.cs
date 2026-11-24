namespace TicketFlow.Guardian.Models;

public sealed class EnrichedFinding
{
    public required string Id { get; init; }
    public List<string> Aliases { get; init; } = [];
    public required string Package { get; init; }
    public required string Version { get; init; }
    public string Ecosystem { get; init; } = "NuGet";
    public double? Cvss { get; init; }
    public double Epss { get; set; }
    public double EpssPercentile { get; set; }
    public bool Kev { get; set; }
    public bool Reachable { get; set; }
    public bool Exposed { get; set; }
    public string Severity { get; init; } = "UNKNOWN";
    public string Summary { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}
