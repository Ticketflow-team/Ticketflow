namespace TicketFlow.Guardian.Services;

public interface ICbomGenerator
{
    string GenerateCbomJson(string repoRoot);
}
