using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Immutable record of a WHYCEPOLICY evaluation decision.
/// Attached to CommandContext by PolicyMiddleware, enforced by ExecutionGuardMiddleware.
/// NO command may execute without an explicit PolicyDecision.
/// </summary>
public sealed record PolicyDecision
{
    public required Guid DecisionId { get; init; }
    public required PolicyDecisionResult Result { get; init; }
    public required IReadOnlyList<Guid> PolicyIds { get; init; }
    public required string EvaluationHash { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? DenialReason { get; init; }
    public IReadOnlyList<string>? Conditions { get; init; }

    /// <summary>
    /// Context key used to store/retrieve the decision in CommandContext.
    /// </summary>
    public const string ContextKey = "Policy.Decision";

    public static PolicyDecision Allow(IReadOnlyList<Guid> policyIds, string evaluationHash, IClock? clock = null)
        => new()
        {
            DecisionId = DeterministicIdHelper.FromSeed($"policy-decision:allow:{evaluationHash}"),
            Result = PolicyDecisionResult.Allow,
            PolicyIds = policyIds,
            EvaluationHash = evaluationHash,
            Timestamp = (clock ?? SystemClock.Instance).UtcNowOffset
        };

    public static PolicyDecision Deny(IReadOnlyList<Guid> policyIds, string evaluationHash, string reason, IClock? clock = null)
        => new()
        {
            DecisionId = DeterministicIdHelper.FromSeed($"policy-decision:deny:{evaluationHash}:{reason}"),
            Result = PolicyDecisionResult.Deny,
            PolicyIds = policyIds,
            EvaluationHash = evaluationHash,
            Timestamp = (clock ?? SystemClock.Instance).UtcNowOffset,
            DenialReason = reason
        };

    public static PolicyDecision Conditional(IReadOnlyList<Guid> policyIds, string evaluationHash, IReadOnlyList<string> conditions, IClock? clock = null)
        => new()
        {
            DecisionId = DeterministicIdHelper.FromSeed($"policy-decision:conditional:{evaluationHash}"),
            Result = PolicyDecisionResult.Conditional,
            PolicyIds = policyIds,
            EvaluationHash = evaluationHash,
            Timestamp = (clock ?? SystemClock.Instance).UtcNowOffset,
            Conditions = conditions
        };

    /// <summary>
    /// Computes a deterministic hash from evaluation inputs for audit verification.
    /// </summary>
    public static string ComputeEvaluationHash(Guid commandId, string commandType, string correlationId)
    {
        var input = $"{commandId}:{commandType}:{correlationId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }
}

public enum PolicyDecisionResult
{
    Allow,
    Deny,
    Conditional
}
