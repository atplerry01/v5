using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Routing.Execution;

public sealed record StartExecutionCommand(
    Guid ExecutionId,
    Guid PathId) : IHasAggregateId
{
    public Guid AggregateId => ExecutionId;
}

public sealed record CompleteExecutionCommand(Guid ExecutionId) : IHasAggregateId
{
    public Guid AggregateId => ExecutionId;
}

public sealed record FailExecutionCommand(
    Guid ExecutionId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => ExecutionId;
}

public sealed record AbortExecutionCommand(
    Guid ExecutionId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => ExecutionId;
}
