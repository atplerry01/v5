namespace Whycespace.Platform.Api.Business.Entitlement.Eligibility;

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
