namespace Whycespace.Projections.Intelligence.Search.Synonym;

public sealed record SynonymView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
