namespace Whycespace.Platform.Api.Operational.Sandbox.Todo;

public sealed record TodoRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TodoResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
