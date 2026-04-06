namespace Whycespace.Projections.Intelligence.Knowledge.Ontology;

public sealed record OntologyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
