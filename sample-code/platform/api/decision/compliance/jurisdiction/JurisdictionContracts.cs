namespace Whycespace.Platform.Api.Decision.Compliance.Jurisdiction;

public sealed record JurisdictionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record JurisdictionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
