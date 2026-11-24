namespace TicketFlow.Guardian.Commands;

using TicketFlow.Guardian.Services;

public class CbomCommand
{
    private readonly ICbomGenerator _generator;

    public CbomCommand(ICbomGenerator? generator = null)
    {
        _generator = generator ?? new CbomGenerator();
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var outputPath = "cbom.cdx.json";
        var repoRoot = Directory.GetCurrentDirectory();

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
            else if (arg.Equals("--repo-root", StringComparison.OrdinalIgnoreCase) || arg.Equals("-r", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    repoRoot = args[++i];
                }
            }
        }

        Console.WriteLine("[TicketFlow.Guardian] Generating Cryptographic Bill of Materials (CBOM - P11)");
        Console.WriteLine($"  Target output   : {outputPath}");
        Console.WriteLine($"  Repository root : {repoRoot}");

        var cbomJson = _generator.GenerateCbomJson(repoRoot);
        await File.WriteAllTextAsync(outputPath, cbomJson);

        Console.WriteLine($"[TicketFlow.Guardian] Successfully generated CBOM: {outputPath}");
        Console.WriteLine("  Cryptographic Assets: Ed25519, ML-DSA-44 (PQC), X25519MLKEM768 (TLS PQC), AES-256-GCM, SHA-256");

        return 0;
    }
}
