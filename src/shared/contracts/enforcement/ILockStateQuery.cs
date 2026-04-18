namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Read-side query over the enforcement lock projection. Returns the
/// active lock posture for a subject. Called on the runtime hot path
/// from <c>ExecutionGuardMiddleware</c>, so implementations must:
///
///   - be bounded in latency (single indexed lookup);
///   - return <see cref="ActiveLockState.Unavailable"/> on infrastructure
///     failure (FAIL-CLOSED — locks are safety-critical, unknown state
///     blocks execution rather than permitting it);
///   - never mutate state.
/// </summary>
public interface ILockStateQuery
{
    Task<ActiveLockState> QueryBySubjectAsync(
        Guid subjectId,
        CancellationToken cancellationToken = default);
}
