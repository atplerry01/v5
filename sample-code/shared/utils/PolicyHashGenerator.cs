using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Utils;

/// <summary>
/// Deterministic hash generator for policy decisions.
/// Same input always produces same hash (SHA-256).
/// </summary>
public static class PolicyHashGenerator
{
    public static string ComputeDecisionHash(
        IReadOnlyList<Guid> policyIds,
        string decisionType,
        string? evaluationTrace)
    {
        var sb = new StringBuilder();
        sb.Append("policies:");

        foreach (var id in policyIds.OrderBy(id => id))
            sb.Append(id).Append(';');

        sb.Append("decision:").Append(decisionType);

        if (!string.IsNullOrEmpty(evaluationTrace))
            sb.Append("trace:").Append(evaluationTrace);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexStringLower(bytes);
    }
}
