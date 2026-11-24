namespace TicketFlow.Infrastructure.Demo;

public class DatabaseFacade
{
    public int ExecuteSqlRaw(string sql, params object[] parameters) => 0;
}

public class FakeDbContext
{
    public DatabaseFacade Database { get; } = new();
}

/// <summary>
/// VULNERABILITE VOLONTAIRE POUR DEMONSTRATION JURY (demo/vuln-sqli)
/// Menace T01 / OWASP A05:2025 - Injection SQL
/// Cette classe N'EST JAMAIS fusionnee sur main. Elle prouve que le job SAST Semgrep (P7)
/// bloque la CI et produit une alerte dans l'onglet Security de GitHub.
/// </summary>
public static class VulnerablePaymentQuery
{
    public static void SearchOrdersUnsafe(FakeDbContext dbContext, string userControlledStatus)
    {
        // FAILLE : Interpolation directe de chaîne dans ExecuteSqlRaw
        _ = dbContext.Database.ExecuteSqlRaw($"SELECT * FROM Orders WHERE Status = '{userControlledStatus}'");
    }
}
