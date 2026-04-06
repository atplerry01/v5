namespace Whycespace.Platform.Api.Trust.Identity.Verification;

public sealed record VerificationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record VerificationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
