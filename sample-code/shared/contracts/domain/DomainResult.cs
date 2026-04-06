namespace Whycespace.Shared.Contracts.Domain;

/// <summary>
/// Strongly-typed domain operation result.
/// Replaces DomainOperationResult for governed execution paths.
/// </summary>
public sealed record DomainResult<T>
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public DomainError? Error { get; init; }
    public Guid? AggregateId { get; init; }
    public string? DecisionHash { get; init; }
    public string? ExecutionHash { get; init; }
    public bool ChainAnchored { get; init; }
    public DomainPolicyDecision? Decision { get; init; }

    public static DomainResult<T> Ok(T data, Guid? aggregateId = null) => new()
    {
        Success = true,
        Data = data,
        AggregateId = aggregateId
    };

    public static DomainResult<T> Fail(DomainError error) => new()
    {
        Success = false,
        Error = error
    };

    public static DomainResult<T> PolicyDenied(string reason, string? policyId = null) => new()
    {
        Success = false,
        Error = new DomainError("POLICY_DENIED", reason, policyId)
    };

    /// <summary>
    /// Converts to untyped DomainOperationResult for backward compatibility.
    /// </summary>
    public DomainOperationResult ToUntyped() => Success
        ? DomainOperationResult.Ok(AggregateId, Data)
        : DomainOperationResult.Fail(Error?.Message ?? "Unknown error", Error?.Code);
}

public sealed record DomainError(
    string Code,
    string Message,
    string? PolicyId = null);

/// <summary>
/// Captures the policy decision that governed a domain operation.
/// Embedded in DomainResult for full traceability.
/// </summary>
public sealed record DomainPolicyDecision
{
    public required string DecisionType { get; init; }
    public required bool IsCompliant { get; init; }
    public required bool RequiresAnchoring { get; init; }
    public required ChainAnchoringMode AnchoringMode { get; init; }
    public IReadOnlyList<string> Violations { get; init; } = [];
    public string? DecisionHash { get; init; }
    public IReadOnlyList<Guid> PolicyIds { get; init; } = [];
}
