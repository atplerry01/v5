namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public readonly record struct AuthorizationScope
{
    public Guid PrincipalReference { get; }
    public string ResourceReference { get; }

    public AuthorizationScope(Guid principalReference, string resourceReference)
    {
        if (principalReference == Guid.Empty)
            throw new ArgumentException("PrincipalReference must not be empty.", nameof(principalReference));

        if (string.IsNullOrWhiteSpace(resourceReference))
            throw new ArgumentException("ResourceReference must not be empty.", nameof(resourceReference));

        PrincipalReference = principalReference;
        ResourceReference = resourceReference;
    }
}
