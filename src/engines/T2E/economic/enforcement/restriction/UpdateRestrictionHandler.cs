using Whycespace.Domain.ControlSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

public sealed class UpdateRestrictionHandler : IEngine
{
    private readonly IEnforcementDecisionCache? _cache;

    public UpdateRestrictionHandler() : this(null) { }

    public UpdateRestrictionHandler(IEnforcementDecisionCache? cache)
    {
        _cache = cache;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateRestrictionCommand cmd)
            return;

        if (!Enum.TryParse<RestrictionScope>(cmd.NewScope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown restriction scope: '{cmd.NewScope}'.");

        var aggregate = (RestrictionAggregate)await context.LoadAggregateAsync(typeof(RestrictionAggregate));
        // Phase 7 T7.7 — aggregate rejects update on Suspended or
        // Removed; update is only valid from Applied.
        aggregate.Update(scope, new Reason(cmd.NewReason), new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);

        // Phase 2 — refresh the cache with the updated scope so middleware
        // sees the same authoritative restriction state the aggregate now
        // carries.
        _cache?.RecordRestriction(
            aggregate.SubjectId.Value,
            new ActiveRestrictionState(IsRestricted: true, Scope: cmd.NewScope, Reason: cmd.NewReason));
    }
}
