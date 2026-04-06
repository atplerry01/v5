namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for anchoring policy decisions to WhyceChain.
/// Implementations live in runtime — engines call this via runtime dispatch.
/// Policy is the source of decision; chain stores immutable evidence.
/// </summary>
public interface IPolicyDecisionAnchor
{
    Task<PolicyAnchorResult> AnchorAsync(PolicyAnchorRequest decision, CancellationToken ct = default);
}

/// <summary>
/// Request to anchor a policy decision to WhyceChain.
/// All fields are deterministic and serializable.
/// DecisionHash is the IDEMPOTENT KEY — same logical decision = same hash.
/// CorrelationId = DecisionHash (E4.1 — no timestamp).
/// </summary>
public sealed record PolicyAnchorRequest
{
    public required string PolicyId { get; init; }
    public required string Version { get; init; }
    public required string Decision { get; init; }
    public required string Subject { get; init; }
    public required string Resource { get; init; }
    public required string Action { get; init; }
    public required string ContextHash { get; init; }
    public required string EvaluationHash { get; init; }
    public required string DecisionHash { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    // E5: Identity binding fields
    public required string SubjectId { get; init; }
    public string[] Roles { get; init; } = [];
    public double TrustScore { get; init; }
    public bool IsVerified { get; init; }
    public string? SessionId { get; init; }
    public string? DeviceId { get; init; }

    // E6: Economic binding fields
    public string? AccountId { get; init; }
    public string? AssetId { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? TransactionType { get; init; }

    // E7: Workflow binding fields
    public string? WorkflowId { get; init; }
    public string? StepId { get; init; }
    public string? WorkflowState { get; init; }
    public string? Transition { get; init; }

    /// <summary>
    /// Correlation ID = DecisionHash. Same decision always maps to same correlation.
    /// </summary>
    public string CorrelationId => DecisionHash;
}

/// <summary>
/// Result of a policy decision anchor operation.
/// </summary>
public sealed record PolicyAnchorResult
{
    public required bool Success { get; init; }
    public bool AlreadyAnchored { get; init; }
    public string? BlockId { get; init; }
    public string? BlockHash { get; init; }
    public string? ErrorMessage { get; init; }

    public static PolicyAnchorResult Ok(string blockId, string blockHash)
        => new() { Success = true, BlockId = blockId, BlockHash = blockHash };

    public static PolicyAnchorResult Duplicate(string decisionHash)
        => new() { Success = true, AlreadyAnchored = true, BlockId = null, BlockHash = null };

    public static PolicyAnchorResult Fail(string error)
        => new() { Success = false, ErrorMessage = error };
}
