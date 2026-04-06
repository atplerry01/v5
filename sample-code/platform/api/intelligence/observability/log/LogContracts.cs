namespace Whycespace.Platform.Api.Intelligence.Observability.Log;

public sealed record LogRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record LogResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
