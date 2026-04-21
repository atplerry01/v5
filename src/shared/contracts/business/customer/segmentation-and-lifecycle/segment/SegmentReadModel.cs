namespace Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;

public sealed record SegmentReadModel
{
    public Guid SegmentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Criteria { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
