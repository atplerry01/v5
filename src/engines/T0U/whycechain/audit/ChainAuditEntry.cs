namespace Whyce.Engines.T0U.WhyceChain.Audit;

/// <summary>
/// Immutable audit record for chain operations.
/// </summary>
public sealed record ChainAuditEntry(
    string AuditId,
    Guid CorrelationId,
    string BlockHash,
    string Operation,
    bool IsSuccess,
    long Sequence,
    string? FailureReason);
