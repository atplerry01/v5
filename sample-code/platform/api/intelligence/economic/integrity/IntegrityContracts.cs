namespace Whycespace.Platform.Api.Intelligence.Economic.Integrity;

public sealed record IntegrityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IntegrityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
