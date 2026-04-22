using Whycespace.Domain.ControlSystem.AccessControl.Principal;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Principal;

public sealed class AssignPrincipalRoleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AssignPrincipalRoleCommand cmd)
            return;

        var aggregate = (PrincipalAggregate)await context.LoadAggregateAsync(typeof(PrincipalAggregate));
        aggregate.AssignRole(cmd.RoleId);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
