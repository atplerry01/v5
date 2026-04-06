using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed class ConsentRecordAggregate : AggregateRoot
{
    public ConsentId ConsentId { get; private set; } = default!;
    public IdentityId IdentityId { get; private set; } = default!;
    public ConsentType ConsentType { get; private set; } = default!;
    public ConsentScope Scope { get; private set; } = default!;
    public ConsentStatus Status { get; private set; } = ConsentStatus.Granted;
    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }

    private ConsentRecordAggregate() { }

    public static ConsentRecordAggregate GrantConsent(
        IdentityId identityId,
        ConsentType consentType,
        ConsentScope scope,
        DateTimeOffset timestamp,
        DateTimeOffset? expiryDate = null)
    {
        var consent = new ConsentRecordAggregate
        {
            ConsentId = ConsentId.FromSeed($"ConsentRecord:{identityId.Value}:{consentType.Value}:{scope.Value}"),
            IdentityId = identityId,
            ConsentType = consentType,
            Scope = scope,
            Status = ConsentStatus.Granted,
            GrantedAt = timestamp,
            ExpiryDate = expiryDate
        };

        consent.Id = consent.ConsentId.Value;

        consent.RaiseDomainEvent(new ConsentGrantedEvent(
            consent.ConsentId.Value,
            identityId.Value,
            consentType.Value,
            scope.Value,
            expiryDate));

        return consent;
    }

    public void RevokeConsent(DateTimeOffset timestamp, string reason = "")
    {
        if (Status == ConsentStatus.Revoked)
            throw new InvalidOperationException("Consent is already revoked.");

        if (Status == ConsentStatus.Expired)
            throw new InvalidOperationException("Cannot revoke expired consent.");

        Status = ConsentStatus.Revoked;
        RevokedAt = timestamp;

        RaiseDomainEvent(new ConsentRevokedEvent(
            ConsentId.Value,
            IdentityId.Value,
            reason));
    }

    public void ExpireConsent()
    {
        if (Status != ConsentStatus.Granted)
            throw new InvalidOperationException("Only granted consent can expire.");

        Status = ConsentStatus.Expired;

        RaiseDomainEvent(new ConsentExpiredEvent(
            ConsentId.Value,
            IdentityId.Value));
    }
}
