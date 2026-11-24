namespace TicketFlow.Guardian.Models;

using System.Text.Json.Serialization;

public sealed class OsvReport
{
    [JsonPropertyName("results")]
    public List<OsvResult>? Results { get; set; }
}

public sealed class OsvResult
{
    [JsonPropertyName("source")]
    public OsvSource? Source { get; set; }

    [JsonPropertyName("packages")]
    public List<OsvPackageItem>? Packages { get; set; }
}

public sealed class OsvSource
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public sealed class OsvPackageItem
{
    [JsonPropertyName("package")]
    public OsvPackage? Package { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public List<OsvVulnerability>? Vulnerabilities { get; set; }
}

public sealed class OsvPackage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("ecosystem")]
    public string? Ecosystem { get; set; }
}

public sealed class OsvVulnerability
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    [JsonPropertyName("severity")]
    public List<OsvSeverity>? Severity { get; set; }
}

public sealed class OsvSeverity
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }
}
