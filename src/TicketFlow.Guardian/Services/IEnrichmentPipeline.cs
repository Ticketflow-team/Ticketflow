namespace TicketFlow.Guardian.Services;

using TicketFlow.Guardian.Models;

public interface IEnrichmentPipeline
{
    Task<EnrichedReport> EnrichAsync(
        string osvJsonPath,
        string repoRoot,
        CancellationToken cancellationToken = default);
}
