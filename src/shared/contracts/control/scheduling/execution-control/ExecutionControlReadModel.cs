namespace Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;

public sealed record ExecutionControlReadModel
{
    public Guid ControlId { get; init; }
    public string JobInstanceId { get; init; } = string.Empty;
    public string Signal { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
    public string? Outcome { get; init; }
}
