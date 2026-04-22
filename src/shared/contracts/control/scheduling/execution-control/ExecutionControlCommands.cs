using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;

public sealed record IssueExecutionControlCommand(
    Guid ControlId,
    string JobInstanceId,
    string Signal,
    string ActorId,
    DateTimeOffset IssuedAt) : IHasAggregateId
{
    public Guid AggregateId => ControlId;
}

public sealed record RecordExecutionControlOutcomeCommand(
    Guid ControlId,
    string Outcome) : IHasAggregateId
{
    public Guid AggregateId => ControlId;
}
