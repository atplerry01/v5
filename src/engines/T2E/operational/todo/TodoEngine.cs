using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.Operational.Sandbox.Todo;

namespace Whyce.Engines.T2E.Operational.Todo;

public sealed class TodoEngine : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        switch (context.Command)
        {
            case CreateTodoCommand cmd:
            {
                var aggregate = TodoAggregate.Create(
                    new TodoId(context.AggregateId),
                    cmd.Title);
                context.EmitEvents(aggregate.DomainEvents);
                break;
            }

            case UpdateTodoCommand cmd:
            {
                var aggregate = (TodoAggregate)await context.LoadAggregateAsync(typeof(TodoAggregate));
                aggregate.Update(cmd.Title);
                context.EmitEvents(aggregate.DomainEvents);
                break;
            }

            case CompleteTodoCommand:
            {
                var aggregate = (TodoAggregate)await context.LoadAggregateAsync(typeof(TodoAggregate));
                aggregate.Complete();
                context.EmitEvents(aggregate.DomainEvents);
                break;
            }
        }
    }
}
