namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed class EntitlementScope
{
    public string ResourceRef { get; }
    public string Permission { get; }

    private EntitlementScope(string resourceRef, string permission)
    {
        ResourceRef = resourceRef;
        Permission = permission;
    }

    public static EntitlementScope Create(string resourceRef, string permission)
    {
        if (string.IsNullOrWhiteSpace(resourceRef)) throw EntitlementErrors.InvalidScope();
        if (string.IsNullOrWhiteSpace(permission)) throw EntitlementErrors.InvalidScope();
        return new EntitlementScope(resourceRef.Trim(), permission.Trim().ToLowerInvariant());
    }

    public string Key => $"{ResourceRef}::{Permission}";
}
