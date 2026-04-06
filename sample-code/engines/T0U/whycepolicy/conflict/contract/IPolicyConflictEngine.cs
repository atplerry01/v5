namespace Whycespace.Engines.T0U.WhycePolicy.Conflict;

public interface IPolicyConflictEngine
{
    Task<ConflictDetectionResult> DetectConflictsAsync(
        Guid policyId,
        CancellationToken cancellationToken = default);
}

public sealed record ConflictDetectionResult(
    bool HasConflicts,
    IReadOnlyList<PolicyConflict> Conflicts);

public sealed record PolicyConflict(
    Guid PolicyAId,
    Guid PolicyBId,
    string RuleConflictDescription);
