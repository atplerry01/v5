namespace Whycespace.Platform.Api.Intelligence.Relationship.Affiliation;

public sealed record AffiliationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AffiliationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
