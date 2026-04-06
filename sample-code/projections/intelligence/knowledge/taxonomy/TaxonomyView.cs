namespace Whycespace.Projections.Intelligence.Knowledge.Taxonomy;

public sealed record TaxonomyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
