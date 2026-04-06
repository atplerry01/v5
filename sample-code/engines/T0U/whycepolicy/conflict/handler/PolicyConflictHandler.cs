namespace Whycespace.Engines.T0U.WhycePolicy.Conflict;

public sealed class PolicyConflictHandler : IPolicyConflictEngine
{
    public Task<ConflictDetectionResult> DetectConflictsAsync(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ConflictDetectionResult(false, []));
    }
}
