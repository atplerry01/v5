using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Deterministic hash of guard execution results for WhyceChain anchoring.
/// Hashes violations only (no timestamps) for reproducibility.
/// </summary>
public static class GuardResultHasher
{
    public static string ComputeGuardHash(GuardExecutionReport report)
    {
        var payload = JsonSerializer.Serialize(
            report.Results.Select(r => new
            {
                guardName = r.GuardName,
                passed = r.Passed,
                violations = r.Violations.Select(v => new
                {
                    rule = v.Rule,
                    severity = v.Severity.ToString(),
                    file = v.File,
                    description = v.Description
                })
            }),
            HashSerializerOptions);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    public static string ComputeCombinedHash(
        GuardExecutionReport? prePolicy,
        GuardExecutionReport? postPolicy)
    {
        var preHash = prePolicy is not null ? ComputeGuardHash(prePolicy) : "none";
        var postHash = postPolicy is not null ? ComputeGuardHash(postPolicy) : "none";
        var combined = $"{preHash}:{postHash}";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexStringLower(hash);
    }

    private static readonly JsonSerializerOptions HashSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
