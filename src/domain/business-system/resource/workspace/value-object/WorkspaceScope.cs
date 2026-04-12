namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public readonly record struct WorkspaceScope
{
    public string Value { get; }

    public WorkspaceScope(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("WorkspaceScope must not be empty.", nameof(value));

        Value = value;
    }
}
