using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

public sealed class RemoveRestrictionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveRestrictionCommand cmd)
            return;

        var aggregate = (RestrictionAggregate)await context.LoadAggregateAsync(typeof(RestrictionAggregate));
        aggregate.Remove(new Reason(cmd.RemovalReason), new Timestamp(cmd.RemovedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
