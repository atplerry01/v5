namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Read-side query over the enforcement restriction projection. Returns the
/// active restriction posture for a subject. Called on the runtime hot path
/// from <c>ExecutionGuardMiddleware</c>, so implementations must:
///
///   - be bounded in latency (single indexed lookup);
///   - treat infrastructure failure as <see cref="ActiveRestrictionState.None"/>
///     (fail-open for availability);
///   - never mutate state.
/// </summary>
public interface IRestrictionStateQuery
{
    Task<ActiveRestrictionState> QueryBySubjectAsync(
        Guid subjectId,
        CancellationToken cancellationToken = default);
}
