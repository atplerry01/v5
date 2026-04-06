using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Utils;

/// <summary>
/// Deterministic hash computation for policy decision anchoring (E4.1 hardened).
///
/// Decision hash (IDEMPOTENT KEY):
///   SHA256(policyId + version + subject + action + resource + contextHash)
///   🚫 MUST NOT include timestamp — same decision always produces same hash.
///
/// Execution hash (NON-IDEMPOTENT):
///   SHA256(decisionHash + timestamp)
///   Unique per execution attempt. Used for audit, NOT for identity.
///
/// Context hash:
///   SHA256(subject + resource + action)
///
/// Correlation ID = decisionHash (no prefix, no truncation).
/// </summary>
public static class PolicyAnchorHashService
{
    /// <summary>
    /// Computes the context hash from evaluation input parameters.
    /// </summary>
    public static string ComputeContextHash(string subject, string resource, string action)
    {
        var input = $"subject:{subject}|resource:{resource}|action:{action}";
        return ComputeSha256(input);
    }

    /// <summary>
    /// Computes the evaluation hash from the full decision data.
    /// </summary>
    public static string ComputeEvaluationHash(
        string policyId, string decision, string contextHash, string? evaluationTrace = null)
    {
        var sb = new StringBuilder();
        sb.Append($"policy:{policyId}|decision:{decision}|context:{contextHash}");
        if (!string.IsNullOrEmpty(evaluationTrace))
            sb.Append($"|trace:{evaluationTrace}");

        return ComputeSha256(sb.ToString());
    }

    /// <summary>
    /// Computes the decision hash — the IDEMPOTENT KEY for a policy decision.
    /// Same logical decision always produces the same hash regardless of when it executes.
    /// E5: Now includes identity fields (subjectId, roles, trustScore, isVerified).
    /// SHA256(policyId + version + subject + action + resource + contextHash + subjectId + roles + trustScore + isVerified).
    /// 🚫 NO timestamp.
    /// </summary>
    public static string ComputeDecisionHash(
        string policyId, string version, string subject, string action, string resource, string contextHash,
        string? subjectId = null, string[]? roles = null, double? trustScore = null, bool? isVerified = null,
        string? accountId = null, string? assetId = null, decimal? amount = null, string? currency = null,
        string? transactionType = null,
        string? workflowId = null, string? stepId = null, string? workflowState = null, string? transition = null)
    {
        var sb = new StringBuilder();
        sb.Append($"{policyId}:{version}:{subject}:{action}:{resource}:{contextHash}");

        // E5: Identity binding — included in hash for identity-bound idempotency
        if (subjectId is not null)
            sb.Append($"|sid:{subjectId}");

        if (roles is not null && roles.Length > 0)
        {
            var sorted = roles.OrderBy(r => r, StringComparer.Ordinal).ToArray();
            sb.Append($"|roles:{string.Join(",", sorted)}");
        }

        if (trustScore.HasValue)
            sb.Append($"|trust:{TrustScoreNormalizer.ToHashString(trustScore.Value)}");

        if (isVerified.HasValue)
            sb.Append($"|verified:{(isVerified.Value ? "true" : "false")}");

        // E6: Economic binding — included in hash for economic-bound idempotency
        if (accountId is not null)
            sb.Append($"|account:{accountId.Trim()}");

        if (assetId is not null)
            sb.Append($"|asset:{assetId.Trim()}");

        if (amount.HasValue && currency is not null)
            sb.Append($"|amount:{AmountNormalizer.ToHashString(amount.Value, currency)}");

        if (currency is not null)
            sb.Append($"|currency:{currency.ToUpperInvariant().Trim()}");

        if (transactionType is not null)
            sb.Append($"|txn:{transactionType.ToLowerInvariant().Trim()}");

        // E7: Workflow binding — included in hash for workflow-bound idempotency
        if (workflowId is not null)
            sb.Append($"|workflow:{workflowId.Trim()}");

        if (stepId is not null)
            sb.Append($"|step:{stepId.Trim()}");

        if (workflowState is not null)
            sb.Append($"|state:{workflowState.ToLowerInvariant().Trim()}");

        if (transition is not null)
            sb.Append($"|transition:{transition.ToLowerInvariant().Trim()}");

        return ComputeSha256(sb.ToString());
    }

    /// <summary>
    /// Computes the execution hash — unique per execution attempt.
    /// SHA256(decisionHash + timestamp). NOT used for identity — used for audit uniqueness.
    /// </summary>
    public static string ComputeExecutionHash(string decisionHash, DateTimeOffset timestamp)
    {
        var input = $"{decisionHash}:{timestamp:O}";
        return ComputeSha256(input);
    }

    /// <summary>
    /// [DEPRECATED — E4.1] Use ComputeDecisionHash instead.
    /// Correlation ID = decisionHash directly. No timestamp, no prefix.
    /// </summary>
    [Obsolete("E4.1: Use ComputeDecisionHash() directly as correlationId. This method included timestamp which broke idempotency.")]
    public static string ComputeCorrelationId(
        string policyId, string version, string subject, string action, DateTimeOffset timestamp)
    {
        var input = $"{policyId}:{version}:{subject}:{action}:{timestamp:O}";
        return $"policy-anchor-{ComputeSha256(input)[..16]}";
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
