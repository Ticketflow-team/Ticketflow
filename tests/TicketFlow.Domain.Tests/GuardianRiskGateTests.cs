using System.Text.Json;
using Xunit;

namespace TicketFlow.Domain.Tests;

public class GuardianRiskGateTests
{
    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "TicketFlow.slnx")) ||
                File.Exists(Path.Combine(current.FullName, "TicketFlow.sln")) ||
                Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    [Fact]
    public void DenyRego_PolicyFile_ExistsAndContainsCoreRiskRules()
    {
        var repoRoot = FindRepoRoot();
        var policyPath = Path.Combine(repoRoot, "policies", "gate", "deny.rego");

        Assert.True(File.Exists(policyPath), $"Policy file not found at {policyPath}");

        var content = File.ReadAllText(policyPath);
        Assert.Contains("package guardian.gate", content);
        Assert.Contains("f.kev == true", content);
        Assert.Contains("f.epss >= 0.10", content);
        Assert.Contains("f.reachable != false", content);
        Assert.Contains("vex_not_affected", content);
        Assert.Contains("D1-KEV", content);
        Assert.Contains("D2-EPSS", content);
        Assert.Contains("D5-SLOPSQUATTING", content);
        Assert.Contains("is_allowed_package", content);
    }

    [Fact]
    public void VexOpenVex_File_ExistsAndHasValidOpenVexSchema()
    {
        var repoRoot = FindRepoRoot();
        var vexPath = Path.Combine(repoRoot, "policies", "vex", "vex.openvex.json");

        Assert.True(File.Exists(vexPath), $"OpenVEX file not found at {vexPath}");

        var json = File.ReadAllText(vexPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("@context", out var contextProp));
        Assert.Contains("openvex", contextProp.GetString() ?? string.Empty);

        Assert.True(root.TryGetProperty("statements", out var statementsProp));
        Assert.Equal(JsonValueKind.Array, statementsProp.ValueKind);
        Assert.True(statementsProp.GetArrayLength() > 0);

        var firstStmt = statementsProp[0];
        Assert.True(firstStmt.TryGetProperty("status", out var statusProp));
        Assert.Equal("not_affected", statusProp.GetString());

        Assert.True(firstStmt.TryGetProperty("vulnerability", out var vulnProp));
        Assert.True(vulnProp.TryGetProperty("id", out _));
    }

    [Fact]
    public void PackageAllowlist_File_ExistsAndHasValidSchema()
    {
        var repoRoot = FindRepoRoot();
        var allowlistPath = Path.Combine(repoRoot, "policies", "gate", "package_allowlist.json");

        Assert.True(File.Exists(allowlistPath), $"Allowlist file not found at {allowlistPath}");

        var json = File.ReadAllText(allowlistPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("allowed_prefixes", out var prefixesProp));
        Assert.Equal(JsonValueKind.Array, prefixesProp.ValueKind);
        Assert.True(prefixesProp.GetArrayLength() > 0);

        Assert.True(root.TryGetProperty("allowed_packages", out var packagesProp));
        Assert.Equal(JsonValueKind.Array, packagesProp.ValueKind);
        Assert.True(packagesProp.GetArrayLength() > 0);
    }

    [Fact]
    public void DenyTestRego_UnitTests_Exist()
    {
        var repoRoot = FindRepoRoot();
        var testPolicyPath = Path.Combine(repoRoot, "policies", "gate", "deny_test.rego");

        Assert.True(File.Exists(testPolicyPath), $"Rego test file not found at {testPolicyPath}");

        var content = File.ReadAllText(testPolicyPath);
        Assert.Contains("test_empty_findings_allowed", content);
        Assert.Contains("test_kev_finding_denied", content);
        Assert.Contains("test_epss_reachable_denied", content);
        Assert.Contains("test_slopsquatting_lockfile_denied", content);
        Assert.Contains("test_allowed_package_lockfile_accepted", content);
    }
}
