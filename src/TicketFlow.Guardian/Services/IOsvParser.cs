namespace TicketFlow.Guardian.Services;

using TicketFlow.Guardian.Models;

public interface IOsvParser
{
    List<EnrichedFinding> Parse(string jsonContent);
    List<EnrichedFinding> ParseFile(string filePath);
}
