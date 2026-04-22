using Whycespace.Domain.ControlSystem.AccessControl.Principal;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Principal;

public sealed class RegisterPrincipalHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterPrincipalCommand cmd)
            return Task.CompletedTask;

        var aggregate = PrincipalAggregate.Register(
            new PrincipalId(cmd.PrincipalId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            Enum.Parse<PrincipalKind>(cmd.Kind, ignoreCase: true),
            cmd.IdentityId);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
