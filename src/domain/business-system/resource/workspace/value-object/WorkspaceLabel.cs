namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public readonly record struct WorkspaceLabel
{
    public string Value { get; }

    public WorkspaceLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("WorkspaceLabel must not be empty.", nameof(value));

        Value = value;
    }
}
