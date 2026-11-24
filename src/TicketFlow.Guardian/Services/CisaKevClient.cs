namespace TicketFlow.Guardian.Services;

using System.Text.Json;
using TicketFlow.Guardian.Models;

public class CisaKevClient : ICisaKevClient
{
    public const string DefaultKevUrl = "https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json";
    private readonly HttpClient _httpClient;
    private readonly string _catalogUrl;

    public CisaKevClient(HttpClient? httpClient = null, string? catalogUrl = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _catalogUrl = catalogUrl ?? DefaultKevUrl;
    }

    public async Task<HashSet<string>> GetKevCveIdsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(_catalogUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"[CisaKev] Warning: CISA KEV returned HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                return [];
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseKevJson(json);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException or JsonException)
        {
            Console.Error.WriteLine($"[CisaKev] Warning: Failed to query CISA KEV ({ex.Message}). Continuing with offline mode.");
            return [];
        }
    }

    public static HashSet<string> ParseKevJson(string json)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json)) return set;

        try
        {
            var catalog = JsonSerializer.Deserialize<CisaKevCatalog>(json);
            if (catalog?.Vulnerabilities == null) return set;

            foreach (var vuln in catalog.Vulnerabilities)
            {
                if (!string.IsNullOrWhiteSpace(vuln.CveId))
                {
                    set.Add(vuln.CveId.Trim());
                }
            }
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[CisaKev] Warning: JSON parsing error in KEV catalog: {ex.Message}");
        }

        return set;
    }
}
