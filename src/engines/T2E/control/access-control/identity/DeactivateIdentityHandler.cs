using Whycespace.Domain.ControlSystem.AccessControl.Identity;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Identity;

public sealed class DeactivateIdentityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeactivateIdentityCommand)
            return;

        var aggregate = (IdentityAggregate)await context.LoadAggregateAsync(typeof(IdentityAggregate));
        aggregate.Deactivate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
