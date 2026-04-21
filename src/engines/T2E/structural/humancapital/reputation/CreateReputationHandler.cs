using Whycespace.Domain.StructuralSystem.Humancapital.Reputation;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Reputation;

public sealed class CreateReputationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateReputationCommand cmd) return Task.CompletedTask;
        var aggregate = ReputationAggregate.Create(
            new ReputationId(cmd.ReputationId),
            new ReputationDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
