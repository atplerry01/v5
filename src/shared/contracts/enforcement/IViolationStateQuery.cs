namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Read-side query over the enforcement violation projection. Returns the
/// active enforcement posture for a subject (keyed by the violation
/// projection's source_reference column). Called on the runtime hot path
/// from <c>ExecutionGuardMiddleware</c>, so implementations must:
///
///   • be bounded in latency (single indexed lookup);
///   • treat infrastructure failure as <see cref="ActiveViolationState.None"/>
///     (fail-open for availability) — the detection worker will re-raise the
///     violation on the next matching event;
///   • never mutate state.
/// </summary>
public interface IViolationStateQuery
{
    Task<ActiveViolationState> QueryBySubjectAsync(
        Guid subjectReference,
        CancellationToken cancellationToken = default);
}
