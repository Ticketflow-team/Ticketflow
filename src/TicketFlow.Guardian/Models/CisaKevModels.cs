namespace TicketFlow.Guardian.Models;

using System.Text.Json.Serialization;

public sealed class CisaKevCatalog
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public List<CisaKevItem>? Vulnerabilities { get; set; }
}

public sealed class CisaKevItem
{
    [JsonPropertyName("cveID")]
    public string? CveId { get; set; }

    [JsonPropertyName("vendorProject")]
    public string? VendorProject { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("vulnerabilityName")]
    public string? VulnerabilityName { get; set; }
}
