using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed class ExpireGrantHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireGrantCommand cmd)
            return;

        var aggregate = (GrantAggregate)await context.LoadAggregateAsync(typeof(GrantAggregate));
        aggregate.Expire(cmd.ExpiredAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
