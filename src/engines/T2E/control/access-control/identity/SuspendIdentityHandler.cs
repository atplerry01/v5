using Whycespace.Domain.ControlSystem.AccessControl.Identity;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Identity;

public sealed class SuspendIdentityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SuspendIdentityCommand cmd)
            return;

        var aggregate = (IdentityAggregate)await context.LoadAggregateAsync(typeof(IdentityAggregate));
        aggregate.Suspend(cmd.Reason);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
