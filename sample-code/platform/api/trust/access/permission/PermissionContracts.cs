namespace Whycespace.Platform.Api.Trust.Access.Permission;

public sealed record PermissionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PermissionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
