namespace Whycespace.Platform.Api.Decision.Compliance.Attestation;

public sealed record AttestationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AttestationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
