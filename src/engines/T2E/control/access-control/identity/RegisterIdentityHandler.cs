using Whycespace.Domain.ControlSystem.AccessControl.Identity;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.Identity;

public sealed class RegisterIdentityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterIdentityCommand cmd)
            return Task.CompletedTask;

        var aggregate = IdentityAggregate.Register(
            new IdentityId(cmd.IdentityId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            Enum.Parse<IdentityKind>(cmd.Kind, ignoreCase: true));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
