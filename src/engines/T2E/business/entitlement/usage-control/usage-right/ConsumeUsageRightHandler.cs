using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.UsageRight;

public sealed class ConsumeUsageRightHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ConsumeUsageRightCommand)
            return;

        var aggregate = (UsageRightAggregate)await context.LoadAggregateAsync(typeof(UsageRightAggregate));
        aggregate.Consume();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
