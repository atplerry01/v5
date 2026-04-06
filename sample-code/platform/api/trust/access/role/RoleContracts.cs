namespace Whycespace.Platform.Api.Trust.Access.Role;

public sealed record RoleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RoleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
