using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Limit;

public sealed class EnforceLimitHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EnforceLimitCommand)
            return;

        var aggregate = (LimitAggregate)await context.LoadAggregateAsync(typeof(LimitAggregate));
        aggregate.Enforce();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
