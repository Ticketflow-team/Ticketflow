namespace TicketFlow.Guardian.Models;

public sealed class EnrichedReport
{
    public DateTime ScanDate { get; init; } = DateTime.UtcNow;
    public string Scanner { get; init; } = "TicketFlow.Guardian";
    public int TotalFindings { get; set; }
    public int KevCount { get; set; }
    public int HighEpssCount { get; set; }
    public int ReachableCount { get; set; }
    public List<EnrichedFinding> Findings { get; init; } = [];
}
