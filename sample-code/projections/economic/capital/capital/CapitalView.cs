namespace Whycespace.Projections.Economic.Capital.Capital;

public sealed record CapitalView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
