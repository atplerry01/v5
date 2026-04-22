using Whycespace.Domain.ControlSystem.AccessControl.Role;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Role;

public sealed class DefineRoleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineRoleCommand cmd)
            return Task.CompletedTask;

        var aggregate = RoleAggregate.Define(
            new RoleId(cmd.RoleId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            cmd.PermissionIds,
            cmd.ParentRoleId);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
