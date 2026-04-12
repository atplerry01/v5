namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed record PermissionDefinedEvent(PermissionId PermissionId, PermissionDescriptor Descriptor);
