namespace Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;

public sealed record IncentiveReadModel
{
    public Guid IncentiveId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
