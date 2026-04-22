namespace Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;

public sealed record SystemJobReadModel
{
    public Guid JobId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public TimeSpan Timeout { get; init; }
    public bool IsDeprecated { get; init; }
}
