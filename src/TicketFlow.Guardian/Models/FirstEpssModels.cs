namespace TicketFlow.Guardian.Models;

using System.Text.Json.Serialization;

public sealed class FirstEpssResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("data")]
    public List<FirstEpssItem>? Data { get; set; }
}

public sealed class FirstEpssItem
{
    [JsonPropertyName("cve")]
    public string? Cve { get; set; }

    [JsonPropertyName("epss")]
    public string? Epss { get; set; }

    [JsonPropertyName("percentile")]
    public string? Percentile { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }
}
