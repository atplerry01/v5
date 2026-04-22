using Whycespace.Domain.ControlSystem.AccessControl.Permission;
using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Permission;

public sealed class DeprecatePermissionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecatePermissionCommand)
            return;

        var aggregate = (PermissionAggregate)await context.LoadAggregateAsync(typeof(PermissionAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
