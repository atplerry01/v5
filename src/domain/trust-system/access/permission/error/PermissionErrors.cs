namespace Whycespace.Domain.TrustSystem.Access.Permission;

public static class PermissionErrors
{
    public static InvalidOperationException MissingId() =>
        new("PermissionId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor() =>
        new("Permission must include a valid descriptor.");

    public static InvalidOperationException InvalidStateTransition(PermissionStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
