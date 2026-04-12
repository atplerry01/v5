namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public readonly record struct VerificationSubject
{
    public Guid IdentityReference { get; }
    public string ClaimType { get; }

    public VerificationSubject(Guid identityReference, string claimType)
    {
        if (identityReference == Guid.Empty)
            throw VerificationErrors.MissingSubject();

        if (string.IsNullOrWhiteSpace(claimType))
            throw VerificationErrors.MissingSubject();

        IdentityReference = identityReference;
        ClaimType = claimType;
    }
}
