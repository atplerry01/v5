using Whycespace.Domain.StructuralSystem.Humancapital.Stewardship;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Stewardship;

public sealed class CreateStewardshipHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateStewardshipCommand cmd) return Task.CompletedTask;
        var aggregate = StewardshipAggregate.Create(
            new StewardshipId(cmd.StewardshipId),
            new StewardshipDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
