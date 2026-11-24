using TicketFlow.Guardian.Commands;

Console.WriteLine("=================================================");
Console.WriteLine(" TicketFlow Guardian CLI — Security Orchestrator ");
Console.WriteLine("=================================================");

if (args.Length == 0 || args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase))
{
    PrintUsage();
    return 0;
}

var commandName = args[0].ToLowerInvariant();

switch (commandName)
{
    case "enrich":
        var enrichCommand = new EnrichCommand();
        return await enrichCommand.ExecuteAsync(args);

    case "--version":
    case "-v":
        Console.WriteLine("TicketFlow.Guardian v1.0.0 (NET 10.0)");
        return 0;

    default:
        Console.Error.WriteLine($"Unknown command: '{args[0]}'");
        PrintUsage();
        return 1;
}

static void PrintUsage()
{
    Console.WriteLine("Usage: dotnet run --project src/TicketFlow.Guardian -- <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  enrich <osv-report.json> [--output <output.json>] [--repo-root <path>]");
    Console.WriteLine("    Enriches OSV-Scanner findings with CISA KEV status, FIRST.org EPSS scores,");
    Console.WriteLine("    and codebase reachability analysis for the OPA Risk Gate (P14).");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --help, -h       Show this help message");
    Console.WriteLine("  --version, -v    Show CLI version");
}
