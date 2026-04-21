namespace Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointReadModel
{
    public Guid ContactPointId { get; init; }
    public Guid CustomerId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsPreferred { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
