namespace Whycespace.Domain.TrustSystem.Access.Permission;

public readonly record struct PermissionDescriptor
{
    public string PermissionName { get; }
    public string ResourceType { get; }

    public PermissionDescriptor(string permissionName, string resourceType)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
            throw new ArgumentException("Permission name must not be empty.", nameof(permissionName));

        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("Resource type must not be empty.", nameof(resourceType));

        PermissionName = permissionName;
        ResourceType = resourceType;
    }
}
