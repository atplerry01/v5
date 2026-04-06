namespace Whycespace.Platform.Api.Structural.Humancapital.Eligibility;

public sealed record EligibilityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EligibilityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
