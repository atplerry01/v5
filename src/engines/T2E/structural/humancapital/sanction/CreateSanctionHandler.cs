using Whycespace.Domain.StructuralSystem.Humancapital.Sanction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Sanction;

public sealed class CreateSanctionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSanctionCommand cmd) return Task.CompletedTask;
        var aggregate = SanctionAggregate.Create(
            new SanctionId(cmd.SanctionId),
            new SanctionDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
