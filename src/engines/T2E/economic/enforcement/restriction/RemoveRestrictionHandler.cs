using Whycespace.Domain.ControlSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

public sealed class RemoveRestrictionHandler : IEngine
{
    private readonly IEnforcementDecisionCache? _cache;

    public RemoveRestrictionHandler() : this(null) { }

    public RemoveRestrictionHandler(IEnforcementDecisionCache? cache)
    {
        _cache = cache;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveRestrictionCommand cmd)
            return;

        var aggregate = (RestrictionAggregate)await context.LoadAggregateAsync(typeof(RestrictionAggregate));
        var subjectId = aggregate.SubjectId.Value;

        aggregate.Remove(new Reason(cmd.RemovalReason), new Timestamp(cmd.RemovedAt));
        context.EmitEvents(aggregate.DomainEvents);

        // Phase 2 — clear the cached restriction so the subject's next
        // command is no longer hard-rejected. The aggregate-carried
        // SubjectId is authoritative (the command only carries RestrictionId).
        _cache?.ClearRestriction(subjectId);
    }
}
