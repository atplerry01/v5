namespace Whycespace.Platform.Api.Intelligence.Knowledge.Taxonomy;

public sealed record TaxonomyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TaxonomyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
