using Whycespace.Domain.ControlSystem.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Authorization;

public sealed class GrantAuthorizationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not GrantAuthorizationCommand cmd)
            return Task.CompletedTask;

        var aggregate = AuthorizationAggregate.Grant(
            new AuthorizationId(cmd.AuthorizationId.ToString("N").PadRight(64, '0')),
            cmd.SubjectId,
            cmd.RoleIds,
            cmd.ValidFrom,
            cmd.ValidTo);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
