namespace TicketFlow.Guardian.Services;

public interface IReachabilityAnalyzer
{
    (bool Reachable, bool Exposed) Analyze(string repoRoot, string packageName);
}
