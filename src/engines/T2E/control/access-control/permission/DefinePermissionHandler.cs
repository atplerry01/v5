using Whycespace.Domain.ControlSystem.AccessControl.Permission;
using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Permission;

public sealed class DefinePermissionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefinePermissionCommand cmd)
            return Task.CompletedTask;

        var aggregate = PermissionAggregate.Define(
            new PermissionId(cmd.PermissionId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            cmd.ResourceScope,
            Enum.Parse<ActionMask>(cmd.Actions, ignoreCase: true));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
