namespace TicketFlow.Guardian.Commands;

using TicketFlow.Guardian.Services;

public class AibomCommand
{
    private readonly IAibomGenerator _generator;

    public AibomCommand(IAibomGenerator? generator = null)
    {
        _generator = generator ?? new AibomGenerator();
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var outputPath = "aibom.cdx.json";

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals("--output", StringComparison.OrdinalIgnoreCase) || arg.Equals("-o", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }
        }

        Console.WriteLine("[TicketFlow.Guardian] Generating AI / Machine Learning Bill of Materials (AIBOM - P11)");
        Console.WriteLine($"  Target output   : {outputPath}");

        var aibomJson = _generator.GenerateAibomJson();
        await File.WriteAllTextAsync(outputPath, aibomJson);

        Console.WriteLine($"[TicketFlow.Guardian] Successfully generated AIBOM: {outputPath}");
        Console.WriteLine("  AI Components: ticketflow-anti-fraud-classifier, ticketflow-security-triage-agent (T24/LLM03)");

        return 0;
    }
}
