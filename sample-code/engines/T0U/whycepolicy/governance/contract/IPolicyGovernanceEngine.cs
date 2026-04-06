using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Governance;

public interface IPolicyGovernanceEngine
{
    Task<GovernanceCheckResult> CheckGovernanceAsync(
        Guid policyId,
        string action,
        CancellationToken cancellationToken = default);

    Task<GovernanceValidationResult> ValidateProposalAsync(
        PolicyProposalInput proposal,
        CancellationToken cancellationToken = default);

    Task<GovernanceValidationResult> ValidateApprovalAsync(
        PolicyApprovalInput approval,
        CancellationToken cancellationToken = default);

    Task<GovernanceActivationResult> ValidateActivationAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Engine-local proposal input — decoupled from domain PolicyProposalAggregate.
/// </summary>
public sealed record PolicyProposalInput
{
    public required Guid Id { get; init; }
    public required Guid PolicyId { get; init; }
    public required Guid ProposedBy { get; init; }
    public required string Metadata { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Engine-local approval input — decoupled from domain PolicyApproval.
/// </summary>
public sealed record PolicyApprovalInput
{
    public required Guid ProposalId { get; init; }
    public required Guid ApproverId { get; init; }
}

public sealed record GovernanceCheckResult(
    bool RequiresApproval,
    bool IsTier0Action,
    string Reason);

/// <summary>
/// Validation result returned by the engine. Contains the data payload
/// needed for the runtime/orchestrator to persist the change.
/// The engine does NOT write — the caller handles persistence.
/// </summary>
public sealed record GovernanceValidationResult
{
    public required bool IsValid { get; init; }
    public string? Error { get; init; }

    /// <summary>Record to persist (proposals, approvals). Null if invalid.</summary>
    public object? PersistencePayload { get; init; }

    public static GovernanceValidationResult Valid(object payload) =>
        new() { IsValid = true, PersistencePayload = payload };

    public static GovernanceValidationResult Invalid(string error) =>
        new() { IsValid = false, Error = error };
}

public sealed record GovernanceActivationResult
{
    public required bool Success { get; init; }
    public Guid PolicyId { get; init; }
    public int Version { get; init; }
    public string? ActivationHash { get; init; }
    public Guid? DraftVersionId { get; init; }
    public Guid? ActivatedBy { get; init; }
    public string? Reason { get; init; }

    public static GovernanceActivationResult Succeeded(
        Guid policyId, int version, string activationHash, Guid draftVersionId, Guid activatedBy) =>
        new()
        {
            Success = true, PolicyId = policyId, Version = version,
            ActivationHash = activationHash, DraftVersionId = draftVersionId, ActivatedBy = activatedBy
        };

    public static GovernanceActivationResult Failed(string reason) =>
        new() { Success = false, Reason = reason };
}

/// <summary>
/// Governance quorum configuration — engine-local type.
/// </summary>
public sealed record GovernanceQuorum
{
    public int RequiredApprovals { get; init; }
    public IReadOnlyList<string> RequiredRoles { get; init; } = [];

    public static GovernanceQuorum Create(int requiredApprovals, IReadOnlyList<string> roles) =>
        new() { RequiredApprovals = requiredApprovals, RequiredRoles = roles };
}
