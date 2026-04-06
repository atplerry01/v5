namespace Whycespace.Platform.Api.Trust.Access.Grant;

public sealed record GrantRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GrantResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
