namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public static class FederationErrors
{
    public static DomainException IssuerNotApproved(Guid issuerId) =>
        new("ISSUER_NOT_APPROVED",
            $"Issuer '{issuerId}' is not in approved status.");

    public static DomainException DuplicateFederationLink(string externalId, Guid issuerId) =>
        new("DUPLICATE_FEDERATION_LINK",
            $"A link already exists for external identity '{externalId}' from issuer '{issuerId}'.");

    public static DomainException InvalidTrustLevel(decimal value) =>
        new("INVALID_TRUST_LEVEL",
            $"Trust level {value} is out of valid range [0, 100].");

    public static DomainException FederationLinkNotFound(string externalId, Guid issuerId) =>
        new("FEDERATION_LINK_NOT_FOUND",
            $"No active link found for external identity '{externalId}' from issuer '{issuerId}'.");

    public static DomainException IssuerAlreadyApproved(Guid issuerId) =>
        new("ISSUER_ALREADY_APPROVED",
            $"Issuer '{issuerId}' is already approved.");

    public static DomainException IssuerNotFound(Guid issuerId) =>
        new("ISSUER_NOT_FOUND",
            $"Issuer '{issuerId}' not found.");
}
