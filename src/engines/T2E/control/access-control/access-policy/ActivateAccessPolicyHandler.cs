using Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.AccessPolicy;

public sealed class ActivateAccessPolicyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateAccessPolicyCommand)
            return;

        var aggregate = (AccessPolicyAggregate)await context.LoadAggregateAsync(typeof(AccessPolicyAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
