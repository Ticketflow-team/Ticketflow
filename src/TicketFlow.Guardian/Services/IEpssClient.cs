namespace TicketFlow.Guardian.Services;

public interface IEpssClient
{
    Task<Dictionary<string, (double Epss, double Percentile)>> GetEpssScoresAsync(
        IEnumerable<string> cveIds,
        CancellationToken cancellationToken = default);
}
