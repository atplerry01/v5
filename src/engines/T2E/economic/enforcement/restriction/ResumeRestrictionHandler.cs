using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.7 — resumes a Suspended restriction back to Applied. The
/// aggregate has retained its original Scope/Reason/Cause throughout
/// the suspension window, so Resume emits a plain transition event and
/// the middleware cache is re-populated from the aggregate's own state.
/// </summary>
public sealed class ResumeRestrictionHandler : IEngine
{
    private readonly IEnforcementDecisionCache? _cache;

    public ResumeRestrictionHandler() : this(null) { }

    public ResumeRestrictionHandler(IEnforcementDecisionCache? cache)
    {
        _cache = cache;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeRestrictionCommand cmd)
            return;

        var aggregate = (RestrictionAggregate)await context.LoadAggregateAsync(typeof(RestrictionAggregate));

        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);

        // Rehydrate the middleware cache from the aggregate's preserved
        // Applied-time Scope / Reason. The aggregate is the authoritative
        // source so the cache cannot drift from domain truth.
        _cache?.RecordRestriction(
            aggregate.SubjectId.Value,
            new ActiveRestrictionState(
                IsRestricted: true,
                Scope: aggregate.Scope.ToString(),
                Reason: aggregate.Reason.Value));
    }
}
