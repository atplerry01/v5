namespace Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;

public sealed record ScheduleControlReadModel
{
    public Guid ScheduleId { get; init; }
    public string JobDefinitionId { get; init; } = string.Empty;
    public string TriggerExpression { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
