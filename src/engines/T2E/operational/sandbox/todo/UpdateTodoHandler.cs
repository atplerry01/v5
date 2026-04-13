using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;

namespace Whyce.Engines.T2E.Operational.Sandbox.Todo;

public sealed class UpdateTodoHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateTodoCommand cmd)
            return;

        var aggregate = (TodoAggregate)await context.LoadAggregateAsync(typeof(TodoAggregate));
        aggregate.ReviseTitle(cmd.Title);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
