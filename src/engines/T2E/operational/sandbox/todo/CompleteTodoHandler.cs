using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;

namespace Whycespace.Engines.T2E.Operational.Sandbox.Todo;

public sealed class CompleteTodoHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteTodoCommand)
            return;

        var aggregate = (TodoAggregate)await context.LoadAggregateAsync(typeof(TodoAggregate));
        aggregate.Complete();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
