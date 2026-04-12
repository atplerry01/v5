namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public sealed record WorkspaceCreatedEvent(
    WorkspaceId WorkspaceId,
    WorkspaceScope Scope,
    WorkspaceLabel Label);
