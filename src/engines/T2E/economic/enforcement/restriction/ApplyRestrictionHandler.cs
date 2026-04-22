using Whycespace.Domain.ControlSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

/// <summary>
/// Applies a restriction. Phase 7 T7.6 added:
/// * Idempotent replay — if the aggregate stream already carries events,
///   the handler short-circuits to a no-op (no duplicate Applied event).
/// * Cause-coupling — maps the command-side <c>EnforcementCauseDto</c>
///   to the validated domain <see cref="EnforcementCause"/>. When the
///   command omits Cause, a Manual cause referencing the SubjectId is
///   synthesized so the aggregate's non-null-cause invariant is honoured
///   without changing the public command contract.
/// </summary>
public sealed class ApplyRestrictionHandler : IEngine
{
    private readonly IEnforcementDecisionCache? _cache;
    private readonly IEventStore _eventStore;

    public ApplyRestrictionHandler(IEventStore eventStore)
        : this(eventStore, cache: null) { }

    public ApplyRestrictionHandler(IEventStore eventStore, IEnforcementDecisionCache? cache)
    {
        _eventStore = eventStore;
        _cache = cache;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ApplyRestrictionCommand cmd)
            return;

        // Idempotent replay: a second ApplyRestrictionCommand with the
        // same RestrictionId collapses to no-op. Prevents duplicate
        // Applied events under at-least-once command redelivery.
        var prior = await _eventStore.LoadEventsAsync(context.AggregateId);
        if (prior.Count > 0)
            return;

        if (!Enum.TryParse<RestrictionScope>(cmd.Scope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown restriction scope: '{cmd.Scope}'.");

        var cause = MapCause(cmd.Cause, cmd.SubjectId);

        var aggregate = RestrictionAggregate.Apply(
            new RestrictionId(cmd.RestrictionId),
            new SubjectId(cmd.SubjectId),
            scope,
            new Reason(cmd.Reason),
            cause,
            new Timestamp(cmd.AppliedAt));

        context.EmitEvents(aggregate.DomainEvents);

        // Phase 2 — populate the decision cache synchronously so the very
        // next command for this subject (in the same process, before the
        // restriction projection materializes) is hard-rejected by
        // ExecutionGuardMiddleware. Closes the projection-lag window.
        _cache?.RecordRestriction(
            cmd.SubjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: cmd.Scope, Reason: cmd.Reason));
    }

    private static EnforcementCause MapCause(EnforcementCauseDto? dto, Guid subjectId)
    {
        if (dto is null)
        {
            // Backward-compatible fallback: callers that haven't been
            // updated to supply a Cause still produce valid aggregates.
            // The cause is recorded as Manual referencing the subject so
            // the audit trail is self-describing.
            return new EnforcementCause(
                EnforcementCauseKind.Manual,
                subjectId,
                "Apply command issued without explicit cause (Phase 7 T7.6 fallback).");
        }

        if (!Enum.TryParse<EnforcementCauseKind>(dto.Kind, ignoreCase: true, out var kind))
            throw new InvalidOperationException($"Unknown EnforcementCauseKind: '{dto.Kind}'.");

        return new EnforcementCause(kind, dto.CauseReferenceId, dto.Detail);
    }
}
