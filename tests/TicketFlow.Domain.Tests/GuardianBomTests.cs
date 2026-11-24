using System.Text.Json;
using TicketFlow.Guardian.Services;
using Xunit;

namespace TicketFlow.Domain.Tests;

public class GuardianBomTests
{
    [Fact]
    public void CbomGenerator_ProducesCompliantCycloneDx16_WithPqcAssets()
    {
        var generator = new CbomGenerator();
        var json = generator.GenerateCbomJson(Directory.GetCurrentDirectory());

        Assert.False(string.IsNullOrWhiteSpace(json));

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("CycloneDX", root.GetProperty("bomFormat").GetString());
        Assert.Equal("1.6", root.GetProperty("specVersion").GetString());

        var components = root.GetProperty("components");
        Assert.True(components.GetArrayLength() >= 5);

        var names = components.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Contains("Ed25519", names);
        Assert.Contains("ML-DSA-44", names);
        Assert.Contains("X25519MLKEM768", names);
        Assert.Contains("AES-256-GCM", names);
        Assert.Contains("SHA-256", names);
    }

    [Fact]
    public void AibomGenerator_ProducesCompliantCycloneDx16_WithModelCards()
    {
        var generator = new AibomGenerator();
        var json = generator.GenerateAibomJson();

        Assert.False(string.IsNullOrWhiteSpace(json));

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("CycloneDX", root.GetProperty("bomFormat").GetString());
        Assert.Equal("1.6", root.GetProperty("specVersion").GetString());

        var components = root.GetProperty("components");
        Assert.Equal(2, components.GetArrayLength());

        var names = components.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Contains("ticketflow-anti-fraud-classifier", names);
        Assert.Contains("ticketflow-security-triage-agent", names);

        foreach (var comp in components.EnumerateArray())
        {
            Assert.Equal("machine-learning-model", comp.GetProperty("type").GetString());
            Assert.True(comp.TryGetProperty("modelCard", out _));
            Assert.True(comp.TryGetProperty("licenses", out _));
        }
    }
}
