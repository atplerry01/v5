using Whycespace.Domain.SharedKernel;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialAggregate : AggregateRoot
{
    public CredentialId CredentialId { get; private set; }
    public Guid IdentityId { get; private set; }
    public CredentialType CredentialType { get; private set; }
    public CredentialStatus Status { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiryDate { get; private set; }

    private CredentialAggregate() { }

    public static CredentialAggregate Issue(
        Guid identityId,
        CredentialType credentialType,
        DateTimeOffset expiryDate,
        DateTimeOffset timestamp)
    {
        var credential = new CredentialAggregate
        {
            CredentialId = CredentialId.FromSeed($"Credential:{identityId}:{credentialType}:{expiryDate:O}"),
            IdentityId = identityId,
            CredentialType = credentialType,
            Status = CredentialStatus.Active,
            IssuedAt = timestamp,
            ExpiryDate = expiryDate
        };

        credential.Id = credential.CredentialId;

        credential.RaiseDomainEvent(new CredentialIssuedEvent(
            credential.CredentialId,
            identityId,
            credentialType,
            credential.IssuedAt,
            expiryDate));

        return credential;
    }

    public void Revoke()
    {
        if (Status == CredentialStatus.Revoked)
            throw new InvalidOperationException("Credential is already revoked.");

        Status = CredentialStatus.Revoked;

        RaiseDomainEvent(new CredentialRevokedEvent(CredentialId, IdentityId));
    }

    public CredentialAggregate Rotate(DateTimeOffset newExpiryDate, DateTimeOffset timestamp)
    {
        if (Status != CredentialStatus.Active)
            throw new InvalidOperationException("Only active credentials can be rotated.");

        Status = CredentialStatus.Revoked;

        var replacement = Issue(IdentityId, CredentialType, newExpiryDate, timestamp);

        replacement.RaiseDomainEvent(new CredentialRotatedEvent(
            CredentialId,
            replacement.CredentialId,
            IdentityId,
            CredentialType,
            newExpiryDate));

        return replacement;
    }
}
