namespace Whycespace.Platform.Api.Business.Resource.Workspace;

public sealed record WorkspaceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record WorkspaceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
