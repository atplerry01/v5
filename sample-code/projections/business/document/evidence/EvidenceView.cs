namespace Whycespace.Projections.Business.Document.Evidence;

public sealed record EvidenceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
