namespace TicketFlow.Guardian.Services;

using System.Text.Json;

public class CbomGenerator : ICbomGenerator
{
    private static readonly JsonSerializerOptions JsonIndentOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string GenerateCbomJson(string repoRoot)
    {
        var cbom = new
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
                    description = "TicketFlow Cryptographic Assets Inventory (CBOM) — Post-Quantum Readiness"
                }
            },
            components = new object[]
            {
                new
                {
                    type = "cryptographic-asset",
                    name = "Ed25519",
                    version = "RFC 8032",
                    description = "Digital signature algorithm for rotating ticket QR verification (TOTP 30s rotation)",
                    cryptoProperties = new
                    {
                        assetType = "algorithm",
                        algorithmProperties = new
                        {
                            primitive = "signature",
                            parameterSetIdentifier = "Ed25519",
                            curve = "edwards25519",
                            executionEnvironment = "software-plain-ram",
                            implementationPlatform = "csharp-dotnet-10",
                            cryptoFunctions = new[] { "sign", "verify" },
                            classicalSecurityLevel = 128,
                            nistQuantumSecurityLevel = 0
                        },
                        oid = "1.3.101.112"
                    }
                },
                new
                {
                    type = "cryptographic-asset",
                    name = "ML-DSA-44",
                    version = "FIPS 204",
                    description = "NIST Post-Quantum Cryptography Module-Lattice Digital Signature Algorithm for server tokens",
                    cryptoProperties = new
                    {
                        assetType = "algorithm",
                        algorithmProperties = new
                        {
                            primitive = "pqc-signature",
                            parameterSetIdentifier = "ML-DSA-44",
                            executionEnvironment = "software-plain-ram",
                            implementationPlatform = "csharp-dotnet-10",
                            cryptoFunctions = new[] { "sign", "verify" },
                            classicalSecurityLevel = 128,
                            nistQuantumSecurityLevel = 2
                        },
                        oid = "2.16.840.1.101.3.4.3.17"
                    }
                },
                new
                {
                    type = "cryptographic-asset",
                    name = "X25519MLKEM768",
                    version = "Draft-IETF",
                    description = "Hybrid Post-Quantum Key Exchange for Gateway TLS (harvest-now-decrypt-later protection)",
                    cryptoProperties = new
                    {
                        assetType = "protocol",
                        algorithmProperties = new
                        {
                            primitive = "key-exchange",
                            parameterSetIdentifier = "X25519MLKEM768",
                            cryptoFunctions = new[] { "key-encapsulation", "key-agreement" },
                            classicalSecurityLevel = 128,
                            nistQuantumSecurityLevel = 3
                        }
                    }
                },
                new
                {
                    type = "cryptographic-asset",
                    name = "AES-256-GCM",
                    version = "NIST SP 800-38D",
                    description = "Authenticated symmetric encryption for sensitive ticket metadata and payloads",
                    cryptoProperties = new
                    {
                        assetType = "algorithm",
                        algorithmProperties = new
                        {
                            primitive = "authenticated-cipher",
                            parameterSetIdentifier = "AES-256-GCM",
                            mode = "GCM",
                            cryptoFunctions = new[] { "encrypt", "decrypt" },
                            classicalSecurityLevel = 256,
                            nistQuantumSecurityLevel = 1
                        }
                    }
                },
                new
                {
                    type = "cryptographic-asset",
                    name = "SHA-256",
                    version = "FIPS 180-4",
                    description = "Secure Hash Algorithm for cryptographic integrity and digest generation",
                    cryptoProperties = new
                    {
                        assetType = "algorithm",
                        algorithmProperties = new
                        {
                            primitive = "hash",
                            parameterSetIdentifier = "SHA-256",
                            cryptoFunctions = new[] { "digest" },
                            classicalSecurityLevel = 128,
                            nistQuantumSecurityLevel = 1
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(cbom, JsonIndentOptions);
    }
}
