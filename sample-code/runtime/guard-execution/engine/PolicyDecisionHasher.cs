using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Whycespace.Runtime.ControlPlane.Policy;

namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Deterministic hash of PolicyDecision for WhyceChain anchoring.
/// Timestamps excluded from hash — only decision-relevant fields are hashed.
/// Stable JSON key ordering via explicit serialization.
/// </summary>
public static class PolicyDecisionHasher
{
    public static string ComputeDecisionHash(PolicyDecision decision)
    {
        // Deterministic payload: no timestamps, stable ordering
        var payload = JsonSerializer.Serialize(new
        {
            decisionId = decision.DecisionId,
            result = decision.Result.ToString(),
            policyIds = decision.PolicyIds.OrderBy(id => id).Select(id => id.ToString()),
            evaluationHash = decision.EvaluationHash,
            denialReason = decision.DenialReason,
            conditions = decision.Conditions
        }, HashSerializerOptions);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    private static readonly JsonSerializerOptions HashSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
