namespace Whycespace.Engines.T0U.WhyceId.Audit;

/// <summary>
/// Immutable audit record for identity operations.
/// </summary>
public sealed record IdentityAuditEntry(
    string AuditId,
    string IdentityId,
    string Operation,
    string OperationHash,
    bool IsSuccess,
    string? FailureReason);
