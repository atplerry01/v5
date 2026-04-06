namespace Whycespace.Platform.Api.Decision.Governance.Charter;

public sealed record CharterRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CharterResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
