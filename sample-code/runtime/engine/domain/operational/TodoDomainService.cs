using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Operational.Todo;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Operational;

public sealed class TodoDomainService : GovernedDomainServiceBase, ITodoDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public TodoDomainService(
        IAggregateStore aggregateStore,
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _aggregateStore = aggregateStore;
    }

    public async Task<DomainOperationResult> CreateAsync(DomainExecutionContext context, Guid todoId, string title, string description, int priority)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var todo = TodoItem.Create(
                todoId,
                title,
                description,
                null,
                priority);

            await _aggregateStore.SaveAsync(todo);

            return (todoId, (object?)new { TodoId = todoId });
        });
    }

    public async Task<DomainOperationResult> CompleteAsync(DomainExecutionContext context, Guid todoId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var id = todoId.ToString();
            var todo = await _aggregateStore.LoadAsync<TodoItem>(id);

            if (todo.Id == Guid.Empty)
                throw new InvalidOperationException($"Todo {todoId} not found.");

            todo.Complete();

            await _aggregateStore.SaveAsync(todo);

            return (todo.Id, (object?)new
            {
                AggregateType = "operational.todo",
                AggregateId = todo.Id.ToString(),
                EventType = "todo.completed",
                EventData = new { todo_id = todo.Id }
            });
        });
    }
}
