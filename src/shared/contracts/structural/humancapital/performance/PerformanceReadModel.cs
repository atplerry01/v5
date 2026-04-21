namespace Whycespace.Shared.Contracts.Structural.Humancapital.Performance;

public sealed record PerformanceReadModel
{
    public Guid PerformanceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
