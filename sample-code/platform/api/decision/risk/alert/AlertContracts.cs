namespace Whycespace.Platform.Api.Decision.Risk.Alert;

public sealed record AlertRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AlertResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
