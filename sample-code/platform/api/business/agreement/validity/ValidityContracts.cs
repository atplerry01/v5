namespace Whycespace.Platform.Api.Business.Agreement.Validity;

public sealed record ValidityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ValidityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
