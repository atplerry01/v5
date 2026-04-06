namespace Whycespace.Platform.Api.Decision.Compliance.Filing;

public sealed record FilingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record FilingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
