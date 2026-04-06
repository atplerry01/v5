namespace Whycespace.Shared.Contracts.Domain;

/// <summary>
/// Enforcement metadata required for governed domain execution.
/// Every domain service call MUST include this context.
/// Engines construct it from EngineContext; runtime validates it.
/// </summary>
public sealed record DomainExecutionContext
{
    public required string CorrelationId { get; init; }
    public required string ActorId { get; init; }
    public required string Action { get; init; }
    public required string Domain { get; init; }
    public string? PolicyId { get; init; }
    public string? PayloadHash { get; init; }
    public string? CommandType { get; init; }
    public Guid? TenantId { get; init; }
    public string? TenantType { get; init; }
    public string? Region { get; init; }
    public string? Jurisdiction { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// When true, policy evaluation is explicitly skipped.
    /// MUST only be set for pure validation/read-only operations (e.g., federation checks).
    /// Logged as an anomaly signal for audit trail.
    /// </summary>
    public bool NoPolicyFlag { get; init; }

    /// <summary>
    /// Validates that all required enforcement fields are present.
    /// Throws if context is incomplete — prevents unguarded execution.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(CorrelationId))
            throw new InvalidDomainExecutionContextException("CorrelationId is required");
        if (string.IsNullOrWhiteSpace(ActorId))
            throw new InvalidDomainExecutionContextException("ActorId is required");
        if (string.IsNullOrWhiteSpace(Action))
            throw new InvalidDomainExecutionContextException("Action is required");
        if (string.IsNullOrWhiteSpace(Domain))
            throw new InvalidDomainExecutionContextException("Domain is required");
    }
}

public sealed class InvalidDomainExecutionContextException : Exception
{
    public InvalidDomainExecutionContextException(string message) : base(message) { }
}
