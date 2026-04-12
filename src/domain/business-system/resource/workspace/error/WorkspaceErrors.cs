namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public static class WorkspaceErrors
{
    public static WorkspaceDomainException MissingId()
        => new("WorkspaceId is required and must not be empty.");

    public static WorkspaceDomainException InvalidStateTransition(WorkspaceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static WorkspaceDomainException ScopeRequired()
        => new("Workspace must define a scope.");

    public static WorkspaceDomainException LabelRequired()
        => new("Workspace must have a label.");
}

public sealed class WorkspaceDomainException : Exception
{
    public WorkspaceDomainException(string message) : base(message) { }
}
