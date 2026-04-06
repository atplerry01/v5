namespace Whycespace.Platform.Api.Intelligence.Knowledge.Ontology;

public sealed record OntologyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record OntologyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
