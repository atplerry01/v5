namespace Whycespace.Projections.Intelligence.Economic._shared;

public sealed record _sharedView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
