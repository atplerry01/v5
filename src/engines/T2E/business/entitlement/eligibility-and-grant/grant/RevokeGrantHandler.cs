using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed class RevokeGrantHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeGrantCommand cmd)
            return;

        var aggregate = (GrantAggregate)await context.LoadAggregateAsync(typeof(GrantAggregate));
        aggregate.Revoke(cmd.RevokedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
