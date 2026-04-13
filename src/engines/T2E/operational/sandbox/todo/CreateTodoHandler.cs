using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;

namespace Whycespace.Engines.T2E.Operational.Sandbox.Todo;

public sealed class CreateTodoHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateTodoCommand cmd)
            return Task.CompletedTask;

        var aggregate = TodoAggregate.Create(
            new TodoId(context.AggregateId),
            cmd.Title);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
