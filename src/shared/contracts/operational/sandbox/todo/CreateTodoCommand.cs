using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Todo;

public sealed record CreateTodoCommand(Guid Id, string Title) : IHasAggregateId
{
    public Guid AggregateId => Id;
}
