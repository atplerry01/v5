using Whycespace.Domain.ControlSystem.AccessControl.Role;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Role;

public sealed class AddRolePermissionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AddRolePermissionCommand cmd)
            return;

        var aggregate = (RoleAggregate)await context.LoadAggregateAsync(typeof(RoleAggregate));
        aggregate.AddPermission(cmd.PermissionId);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
