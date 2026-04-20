using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed class StreamAccessAggregate : AggregateRoot
{
    public StreamAccessId AccessId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public AccessMode Mode { get; private set; }
    public AccessWindow Window { get; private set; }
    public TokenBinding Token { get; private set; }
    public AccessStatus Status { get; private set; }
    public Timestamp GrantedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private StreamAccessAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static StreamAccessAggregate Grant(
        StreamAccessId accessId,
        StreamRef streamRef,
        AccessMode mode,
        AccessWindow window,
        TokenBinding token,
        Timestamp grantedAt)
    {
        Guard.Against(window.HasExpired(grantedAt), "Cannot grant access whose window has already expired.");

        var aggregate = new StreamAccessAggregate();

        aggregate.RaiseDomainEvent(new StreamAccessGrantedEvent(
            accessId,
            streamRef,
            mode,
            window,
            token,
            grantedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Restrict(string reason, Timestamp restrictedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw StreamAccessErrors.InvalidReason();

        if (IsTerminal(Status))
            throw StreamAccessErrors.CannotMutateTerminalAccess();

        if (Status == AccessStatus.Restricted)
            throw StreamAccessErrors.AccessAlreadyRestricted();

        RaiseDomainEvent(new StreamAccessRestrictedEvent(AccessId, reason.Trim(), restrictedAt));
    }

    public void Unrestrict(Timestamp unrestrictedAt)
    {
        if (IsTerminal(Status))
            throw StreamAccessErrors.CannotMutateTerminalAccess();

        if (Status != AccessStatus.Restricted)
            throw StreamAccessErrors.AccessNotRestricted();

        RaiseDomainEvent(new StreamAccessUnrestrictedEvent(AccessId, unrestrictedAt));
    }

    public void Revoke(string reason, Timestamp revokedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw StreamAccessErrors.InvalidReason();

        if (Status == AccessStatus.Revoked)
            throw StreamAccessErrors.AccessAlreadyRevoked();

        if (Status == AccessStatus.Expired)
            throw StreamAccessErrors.AccessAlreadyExpired();

        RaiseDomainEvent(new StreamAccessRevokedEvent(AccessId, reason.Trim(), revokedAt));
    }

    public void Expire(Timestamp expiredAt)
    {
        if (Status == AccessStatus.Revoked)
            throw StreamAccessErrors.AccessAlreadyRevoked();

        if (Status == AccessStatus.Expired)
            throw StreamAccessErrors.AccessAlreadyExpired();

        RaiseDomainEvent(new StreamAccessExpiredEvent(AccessId, expiredAt));
    }

    private static bool IsTerminal(AccessStatus status) =>
        status == AccessStatus.Revoked || status == AccessStatus.Expired;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StreamAccessGrantedEvent e:
                AccessId = e.AccessId;
                StreamRef = e.StreamRef;
                Mode = e.Mode;
                Window = e.Window;
                Token = e.Token;
                Status = AccessStatus.Granted;
                GrantedAt = e.GrantedAt;
                LastModifiedAt = e.GrantedAt;
                break;

            case StreamAccessRestrictedEvent e:
                Status = AccessStatus.Restricted;
                LastModifiedAt = e.RestrictedAt;
                break;

            case StreamAccessUnrestrictedEvent e:
                Status = AccessStatus.Granted;
                LastModifiedAt = e.UnrestrictedAt;
                break;

            case StreamAccessRevokedEvent e:
                Status = AccessStatus.Revoked;
                LastModifiedAt = e.RevokedAt;
                break;

            case StreamAccessExpiredEvent e:
                Status = AccessStatus.Expired;
                LastModifiedAt = e.ExpiredAt;
                break;
        }
    }
}
