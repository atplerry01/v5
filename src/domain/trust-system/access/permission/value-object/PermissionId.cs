namespace Whycespace.Domain.TrustSystem.Access.Permission;

public readonly record struct PermissionId
{
    public Guid Value { get; }

    public PermissionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermissionId must not be empty.", nameof(value));

        Value = value;
    }
}
