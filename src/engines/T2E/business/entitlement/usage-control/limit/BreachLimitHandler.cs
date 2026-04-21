using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Limit;

public sealed class BreachLimitHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not BreachLimitCommand cmd)
            return;

        var aggregate = (LimitAggregate)await context.LoadAggregateAsync(typeof(LimitAggregate));
        aggregate.Breach(cmd.ObservedValue);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
