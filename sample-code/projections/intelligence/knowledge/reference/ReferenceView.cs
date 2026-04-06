namespace Whycespace.Projections.Intelligence.Knowledge.Reference;

public sealed record ReferenceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
