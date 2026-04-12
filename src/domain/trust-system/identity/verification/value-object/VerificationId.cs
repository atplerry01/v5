namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public readonly record struct VerificationId
{
    public Guid Value { get; }

    public VerificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw VerificationErrors.MissingId();

        Value = value;
    }
}
