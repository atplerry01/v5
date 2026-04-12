namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public readonly record struct IdentityDescriptor
{
    public string PrincipalName { get; }
    public string PrincipalType { get; }

    public IdentityDescriptor(string principalName, string principalType)
    {
        if (string.IsNullOrWhiteSpace(principalName))
            throw new ArgumentException("PrincipalName must not be empty.", nameof(principalName));

        if (string.IsNullOrWhiteSpace(principalType))
            throw new ArgumentException("PrincipalType must not be empty.", nameof(principalType));

        PrincipalName = principalName;
        PrincipalType = principalType;
    }
}
