namespace TicketFlow.Guardian.Services;

using System.Text.Json;

public class AibomGenerator : IAibomGenerator
{
    private static readonly JsonSerializerOptions JsonIndentOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string GenerateAibomJson()
    {
        var aibom = new
        {
            bomFormat = "CycloneDX",
            specVersion = "1.6",
            serialNumber = $"urn:uuid:{Guid.NewGuid()}",
            version = 1,
            metadata = new
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                tools = new
                {
                    components = new[]
                    {
                        new
                        {
                            type = "application",
                            name = "TicketFlow.Guardian",
                            version = "1.0.0"
                        }
                    }
                },
                component = new
                {
                    type = "application",
                    name = "TicketFlow.Api",
                    version = "1.0.0",
                    description = "TicketFlow AI / Machine Learning Bill of Materials (AIBOM) — T30 & LLM03 Supply Chain"
                    }
                },
                components = new object[]
                {
                    new
                    {
                        type = "machine-learning-model",
                        name = "ticketflow-anti-fraud-classifier",
                        version = "gemini-4-hydrogen",
                        description = "External Google Gemini API classifier for real-time ticket scalping and transaction fraud evaluation (T26-T30 / LLM03)",
                        licenses = new[]
                        {
                            new { license = new { name = "Google Gemini API Terms of Service" } }
                        },
                        modelCard = new
                        {
                            modelDetails = new
                            {
                                type = "generative-language-model-api",
                                provider = "Google Gemini API",
                                pinnedModel = "gemini-4-hydrogen",
                                deploymentMode = "external-https-api",
                                description = "Advisory anti-scalping and transaction anomaly scoring; deterministic C# rules in TicketFlow.Application make final decisions",
                                version = "gemini-4-hydrogen"
                            },
                            inputs = new[]
                            {
                                new { format = "numeric", description = "Transaction velocity (purchases per minute)" },
                                new { format = "numeric", description = "IP reputation risk index" },
                                new { format = "string", description = "Pseudonymized user behavior anomaly sequence (no PII or PAN)" }
                            },
                            outputs = new[]
                            {
                                new { format = "float", description = "Advisory fraud risk score between 0.0 and 1.0" },
                                new { format = "string", description = "Advisory recommendation: ALLOW, CHALLENGE_MFA, DENY" }
                            },
                            governance = new
                            {
                                owners = new[]
                                {
                                    new { name = "TicketFlow Security & AI Team (Student D)" }
                                }
                            }
                        }
                    },
                    new
                    {
                        type = "machine-learning-model",
                        name = "ticketflow-security-triage-agent",
                        version = "gemini-4-hydrogen",
                        description = "External Google Gemini API agent for DevSecOps finding triage, remediation proposals, and VEX drafts (P16 / T26-T30)",
                        licenses = new[]
                        {
                            new { license = new { name = "Google Gemini API Terms of Service" } }
                        },
                        modelCard = new
                        {
                            modelDetails = new
                            {
                                type = "generative-language-model-api",
                                provider = "Google Gemini API",
                                pinnedModel = "gemini-4-hydrogen",
                                deploymentMode = "external-https-api",
                                description = "Bounded AI security copilot with zero merge permissions; cannot override policies/gate/deny.rego",
                                version = "gemini-4-hydrogen"
                            },
                            inputs = new[]
                            {
                                new { format = "json", description = "Redacted security findings (findings.enriched.json / SARIF without secrets or PII)" }
                            },
                            outputs = new[]
                            {
                                new { format = "json", description = "Triage summary, remediation proposal, and draft OpenVEX explanation for human review" }
                            },
                            governance = new
                            {
                                owners = new[]
                                {
                                    new { name = "TicketFlow Security & AI Team (Student D)" }
                                }
                            }
                        }
                    }
                }
            };

        return JsonSerializer.Serialize(aibom, JsonIndentOptions);
    }
}
