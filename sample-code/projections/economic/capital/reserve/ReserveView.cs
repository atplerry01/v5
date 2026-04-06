namespace Whycespace.Projections.Economic.Capital.Reserve;

public sealed record ReserveView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
