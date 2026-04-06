namespace Whycespace.Projections.Economic.Transaction.Limit;

public sealed record LimitView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
