using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Renewal;

public sealed class ExpireRenewalHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireRenewalCommand)
            return;

        var aggregate = (RenewalAggregate)await context.LoadAggregateAsync(typeof(RenewalAggregate));
        aggregate.Expire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
