using Whycespace.Domain.StructuralSystem.Humancapital.Governance;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Governance;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Governance;

public sealed class CreateGovernanceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateGovernanceCommand cmd) return Task.CompletedTask;
        var aggregate = GovernanceAggregate.Create(
            new GovernanceId(cmd.GovernanceId),
            new GovernanceDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
