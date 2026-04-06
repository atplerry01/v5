namespace Whycespace.Platform.Api.Constitutional.Policy.Scope;

public sealed record ScopeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ScopeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
