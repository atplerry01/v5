namespace Whycespace.Engines.T2E.Business.Resource.Workspace;

public record WorkspaceCommand(
    string Action,
    string EntityId,
    object Payload
);
