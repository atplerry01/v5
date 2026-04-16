namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Read-side query over the enforcement escalation projection. Returns the
/// current escalation posture for a subject. Called on the runtime hot path
/// from <c>ExecutionGuardMiddleware</c>, so implementations must:
///
///   • be bounded in latency (single indexed PK lookup);
///   • treat infrastructure failure as <see cref="ActiveEscalationState.None"/>
///     (fail-open for availability);
///   • never mutate state.
/// </summary>
public interface IEscalationStateQuery
{
    Task<ActiveEscalationState> QueryBySubjectAsync(
        Guid subjectId,
        CancellationToken cancellationToken = default);
}
