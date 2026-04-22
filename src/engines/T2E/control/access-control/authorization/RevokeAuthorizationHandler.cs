using Whycespace.Domain.ControlSystem.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Authorization;

public sealed class RevokeAuthorizationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeAuthorizationCommand)
            return;

        var aggregate = (AuthorizationAggregate)await context.LoadAggregateAsync(typeof(AuthorizationAggregate));
        aggregate.Revoke();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
