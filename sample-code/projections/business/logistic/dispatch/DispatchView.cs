namespace Whycespace.Projections.Business.Logistic.Dispatch;

public sealed record DispatchView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
