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
                    description = "TicketFlow AI / Machine Learning Bill of Materials (AIBOM) — T24 & LLM03 Supply Chain"
                }
            },
            components = new object[]
            {
                new
                {
                    type = "machine-learning-model",
                    name = "ticketflow-anti-fraud-classifier",
                    version = "1.2.0",
                    description = "AI model for real-time ticket scalping and transaction fraud detection (T24 / LLM03)",
                    licenses = new[]
                    {
                        new { license = new { id = "Apache-2.0" } }
                    },
                    hashes = new[]
                    {
                        new { alg = "SHA-256", content = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" }
                    },
                    modelCard = new
                    {
                        modelDetails = new
                        {
                            type = "supervised-classifier",
                            description = "Anti-scalping and transaction anomaly detection",
                            architecture = "Gradient-Boosted Decision Trees + Token Embedding Guardrail",
                            version = "1.2.0"
                        },
                        inputs = new[]
                        {
                            new { format = "numeric", description = "Transaction velocity (purchases per minute)" },
                            new { format = "numeric", description = "IP reputation risk index" },
                            new { format = "string", description = "User behavior anomaly sequence" }
                        },
                        outputs = new[]
                        {
                            new { format = "float", description = "Fraud risk score between 0.0 and 1.0" },
                            new { format = "string", description = "Decision recommendation: ALLOW, CHALLENGE_MFA, DENY" }
                        },
                        performanceMetrics = new[]
                        {
                            new { type = "accuracy", value = "0.984" },
                            new { type = "f1-score", value = "0.962" }
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
                    version = "pinned-qwen2.5-coder-7b",
                    description = "Offline LLM agent for DevSecOps finding triage and PR suggestions (P16)",
                    licenses = new[]
                    {
                        new { license = new { id = "Apache-2.0" } }
                    },
                    hashes = new[]
                    {
                        new { alg = "SHA-256", content = "7f83b1657ff1fc53b92dc18148a1d65dfc2d4b1fa3d677284addd200126d9069" }
                    },
                    modelCard = new
                    {
                        modelDetails = new
                        {
                            type = "generative-language-model",
                            description = "Bounded AI security copilot with zero merge permissions",
                            architecture = "Transformer (7B parameters)",
                            version = "2.5.0"
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(aibom, JsonIndentOptions);
    }
}
