namespace Whycespace.Platform.Api.Intelligence.Identity.IdentityIntelligence;

public sealed record IdentityIntelligenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IdentityIntelligenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
