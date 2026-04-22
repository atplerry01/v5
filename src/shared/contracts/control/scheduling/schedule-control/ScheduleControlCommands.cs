using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;

public sealed record DefineScheduleControlCommand(
    Guid ScheduleId,
    string JobDefinitionId,
    string TriggerExpression) : IHasAggregateId
{
    public Guid AggregateId => ScheduleId;
}

public sealed record SuspendScheduleControlCommand(
    Guid ScheduleId) : IHasAggregateId
{
    public Guid AggregateId => ScheduleId;
}

public sealed record ResumeScheduleControlCommand(
    Guid ScheduleId) : IHasAggregateId
{
    public Guid AggregateId => ScheduleId;
}

public sealed record RetireScheduleControlCommand(
    Guid ScheduleId) : IHasAggregateId
{
    public Guid AggregateId => ScheduleId;
}
