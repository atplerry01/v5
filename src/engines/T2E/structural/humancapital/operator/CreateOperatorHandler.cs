using Whycespace.Domain.StructuralSystem.Humancapital.Operator;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Operator;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Operator;

public sealed class CreateOperatorHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateOperatorCommand cmd) return Task.CompletedTask;
        var aggregate = OperatorAggregate.Create(
            new OperatorId(cmd.OperatorId),
            new OperatorDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
