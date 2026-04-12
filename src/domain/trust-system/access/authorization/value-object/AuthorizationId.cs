namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public readonly record struct AuthorizationId
{
    public Guid Value { get; }

    public AuthorizationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuthorizationId must not be empty.", nameof(value));

        Value = value;
    }
}
