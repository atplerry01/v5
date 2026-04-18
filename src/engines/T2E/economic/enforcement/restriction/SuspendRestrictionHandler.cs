using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.7 — suspends an Applied restriction for a bounded cause
/// (typically a compensation flow against the same subject). The
/// aggregate rejects suspend on any state other than Applied, so the
/// handler simply loads and delegates.
///
/// Decision-cache side effect: clears the subject's cached restriction
/// so middleware stops short-rejecting commands tied to the suspension
/// cause. Resume restores the cache entry from the aggregate's
/// preserved Apply-time state.
/// </summary>
public sealed class SuspendRestrictionHandler : IEngine
{
    private readonly IEnforcementDecisionCache? _cache;

    public SuspendRestrictionHandler() : this(null) { }

    public SuspendRestrictionHandler(IEnforcementDecisionCache? cache)
    {
        _cache = cache;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SuspendRestrictionCommand cmd)
            return;

        if (!Enum.TryParse<EnforcementCauseKind>(cmd.SuspensionCause.Kind, ignoreCase: true, out var kind))
            throw new InvalidOperationException($"Unknown EnforcementCauseKind: '{cmd.SuspensionCause.Kind}'.");

        var suspensionCause = new EnforcementCause(
            kind,
            cmd.SuspensionCause.CauseReferenceId,
            cmd.SuspensionCause.Detail);

        var aggregate = (RestrictionAggregate)await context.LoadAggregateAsync(typeof(RestrictionAggregate));
        var subjectId = aggregate.SubjectId.Value;

        aggregate.Suspend(suspensionCause, new Timestamp(cmd.SuspendedAt));
        context.EmitEvents(aggregate.DomainEvents);

        // Middleware sees the subject as unrestricted for the duration
        // of the suspension. The aggregate preserves its Applied-time
        // Scope/Reason/Cause so Resume reconstitutes them exactly.
        _cache?.ClearRestriction(subjectId);
    }
}
