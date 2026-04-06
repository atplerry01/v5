namespace Whycespace.Projections.Business.Logistic.Handoff;

public sealed record HandoffView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
