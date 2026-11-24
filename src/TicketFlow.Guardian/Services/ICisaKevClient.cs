namespace TicketFlow.Guardian.Services;

public interface ICisaKevClient
{
    Task<HashSet<string>> GetKevCveIdsAsync(CancellationToken cancellationToken = default);
}
