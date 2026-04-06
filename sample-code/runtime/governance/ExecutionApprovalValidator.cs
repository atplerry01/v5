using Whycespace.Shared.Contracts.Domain.Structural;

namespace Whycespace.Runtime.Governance;

/// <summary>
/// Pre-execution governance validation (E18.4).
/// Validates that governance context meets minimum requirements
/// before passing to WHYCEPOLICY for decision.
///
/// CRITICAL: This validator gates execution — it does NOT make governance decisions.
/// Policy remains the only authority.
/// </summary>
public sealed class ExecutionApprovalValidator
{
    public ExecutionApprovalResult Validate(IGovernanceExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ExecutionPath == null || context.ExecutionPath.Count == 0)
            return ExecutionApprovalResult.Rejected(
                context.RootEntityId, "Execution path required");

        if (string.IsNullOrWhiteSpace(context.GovernanceScope))
            return ExecutionApprovalResult.Rejected(
                context.RootEntityId, "Governance scope required");

        return ExecutionApprovalResult.Approved(
            context.RootEntityId, context.ExecutionPath);
    }
}

public sealed record ExecutionApprovalResult
{
    public required Guid RootEntityId { get; init; }
    public required bool IsApproved { get; init; }
    public string? RejectionReason { get; init; }
    public IReadOnlyCollection<Guid>? ApprovedPath { get; init; }

    public static ExecutionApprovalResult Approved(Guid rootEntityId, IReadOnlyCollection<Guid> path) =>
        new()
        {
            RootEntityId = rootEntityId,
            IsApproved = true,
            ApprovedPath = path
        };

    public static ExecutionApprovalResult Rejected(Guid rootEntityId, string reason) =>
        new()
        {
            RootEntityId = rootEntityId,
            IsApproved = false,
            RejectionReason = reason
        };
}
