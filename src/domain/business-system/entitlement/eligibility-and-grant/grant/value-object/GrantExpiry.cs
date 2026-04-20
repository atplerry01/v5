namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantExpiry
{
    public DateTimeOffset ExpiresAt { get; }

    public GrantExpiry(DateTimeOffset expiresAt)
    {
        ExpiresAt = expiresAt;
    }

    public bool IsExpiredAt(DateTimeOffset at) => at >= ExpiresAt;
}
