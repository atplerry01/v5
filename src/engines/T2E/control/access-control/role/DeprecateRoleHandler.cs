using Whycespace.Domain.ControlSystem.AccessControl.Role;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Role;

public sealed class DeprecateRoleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateRoleCommand)
            return;

        var aggregate = (RoleAggregate)await context.LoadAggregateAsync(typeof(RoleAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
