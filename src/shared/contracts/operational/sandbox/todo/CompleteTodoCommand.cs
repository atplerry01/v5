using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Todo;

public sealed record CompleteTodoCommand(Guid Id) : IHasAggregateId
{
    public Guid AggregateId => Id;
}
