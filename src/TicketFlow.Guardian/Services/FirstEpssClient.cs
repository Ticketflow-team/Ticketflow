namespace TicketFlow.Guardian.Services;

using System.Globalization;
using System.Text.Json;
using TicketFlow.Guardian.Models;

public class FirstEpssClient : IEpssClient
{
    public const string DefaultBaseUrl = "https://api.first.org/data/v1/epss";
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public FirstEpssClient(HttpClient? httpClient = null, string? baseUrl = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _baseUrl = baseUrl ?? DefaultBaseUrl;
    }

    public async Task<Dictionary<string, (double Epss, double Percentile)>> GetEpssScoresAsync(
        IEnumerable<string> cveIds,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, (double Epss, double Percentile)>(StringComparer.OrdinalIgnoreCase);

        var validCves = cveIds
            .Where(id => !string.IsNullOrWhiteSpace(id) && id.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (validCves.Count == 0)
        {
            return result;
        }

        // Batch by 50 to avoid long URI queries
        const int batchSize = 50;
        for (var i = 0; i < validCves.Count; i += batchSize)
        {
            var batch = validCves.Skip(i).Take(batchSize).ToList();
            var cveParam = string.Join(",", batch);
            var url = $"{_baseUrl}?cve={Uri.EscapeDataString(cveParam)}";

            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"[FirstEpss] Warning: EPSS API returned HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var batchScores = ParseEpssResponse(json);
                foreach (var (k, v) in batchScores)
                {
                    result[k] = v;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException or JsonException)
            {
                Console.Error.WriteLine($"[FirstEpss] Warning: Failed to query EPSS for batch ({ex.Message}). Continuing.");
            }
        }

        return result;
    }

    public static Dictionary<string, (double Epss, double Percentile)> ParseEpssResponse(string json)
    {
        var scores = new Dictionary<string, (double Epss, double Percentile)>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json)) return scores;

        try
        {
            var response = JsonSerializer.Deserialize<FirstEpssResponse>(json);
            if (response?.Data == null) return scores;

            foreach (var item in response.Data)
            {
                if (string.IsNullOrWhiteSpace(item.Cve)) continue;

                var epss = 0.0;
                var percentile = 0.0;

                if (!string.IsNullOrWhiteSpace(item.Epss) &&
                    double.TryParse(item.Epss, CultureInfo.InvariantCulture, out var parsedEpss))
                {
                    epss = parsedEpss;
                }

                if (!string.IsNullOrWhiteSpace(item.Percentile) &&
                    double.TryParse(item.Percentile, CultureInfo.InvariantCulture, out var parsedPercentile))
                {
                    percentile = parsedPercentile;
                }

                scores[item.Cve.Trim()] = (epss, percentile);
            }
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[FirstEpss] Warning: Failed to parse EPSS response: {ex.Message}");
        }

        return scores;
    }
}
