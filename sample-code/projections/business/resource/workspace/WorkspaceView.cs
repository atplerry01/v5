namespace Whycespace.Projections.Business.Resource.Workspace;

public sealed record WorkspaceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
