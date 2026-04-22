using Whycespace.Domain.ControlSystem.AccessControl.Principal;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Principal;

public sealed class DeactivatePrincipalHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeactivatePrincipalCommand)
            return;

        var aggregate = (PrincipalAggregate)await context.LoadAggregateAsync(typeof(PrincipalAggregate));
        aggregate.Deactivate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
