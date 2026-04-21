using Whycespace.Domain.StructuralSystem.Humancapital.Performance;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Performance;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Performance;

public sealed class CreatePerformanceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreatePerformanceCommand cmd) return Task.CompletedTask;
        var aggregate = PerformanceAggregate.Create(
            new PerformanceId(cmd.PerformanceId),
            new PerformanceDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
