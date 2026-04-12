namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public readonly record struct WorkspaceId
{
    public Guid Value { get; }

    public WorkspaceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkspaceId value must not be empty.", nameof(value));

        Value = value;
    }
}
