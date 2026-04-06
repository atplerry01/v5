namespace Whycespace.Platform.Api.Decision.Compliance.Regulation;

public sealed record RegulationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RegulationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
