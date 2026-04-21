using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed class GrantAggregate : AggregateRoot
{
    public GrantId Id { get; private set; }
    public GrantSubjectRef Subject { get; private set; }
    public GrantTargetRef Target { get; private set; }
    public GrantScope Scope { get; private set; }
    public GrantStatus Status { get; private set; }
    public GrantExpiry? Expiry { get; private set; }

    public static GrantAggregate Create(
        GrantId id,
        GrantSubjectRef subject,
        GrantTargetRef target,
        GrantScope scope,
        DateTimeOffset now,
        GrantExpiry? expiry = null)
    {
        if (expiry.HasValue && expiry.Value.IsExpiredAt(now))
            throw GrantErrors.ExpiryInPast();

        var aggregate = new GrantAggregate();
        if (aggregate.Version >= 0)
            throw GrantErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new GrantCreatedEvent(id, subject, target, scope, expiry));
        return aggregate;
    }

    public void Activate(DateTimeOffset activatedAt)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw GrantErrors.AlreadyTerminal(Id, Status);

        if (Expiry.HasValue && Expiry.Value.IsExpiredAt(activatedAt))
            throw GrantErrors.ExpiryInPast();

        RaiseDomainEvent(new GrantActivatedEvent(Id, activatedAt));
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw GrantErrors.InvalidStateTransition(Status, nameof(Revoke));

        RaiseDomainEvent(new GrantRevokedEvent(Id, revokedAt));
    }

    public void Expire(DateTimeOffset expiredAt)
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw GrantErrors.InvalidStateTransition(Status, nameof(Expire));

        if (Expiry is null)
            throw GrantErrors.InvalidStateTransition(Status, $"{nameof(Expire)}-without-expiry");

        if (!Expiry.Value.IsExpiredAt(expiredAt))
            throw GrantErrors.InvalidStateTransition(Status, $"{nameof(Expire)}-before-expiry");

        RaiseDomainEvent(new GrantExpiredEvent(Id, expiredAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case GrantCreatedEvent e:
                Id = e.GrantId;
                Subject = e.Subject;
                Target = e.Target;
                Scope = e.Scope;
                Expiry = e.Expiry;
                Status = GrantStatus.Pending;
                break;
            case GrantActivatedEvent:
                Status = GrantStatus.Active;
                break;
            case GrantRevokedEvent:
                Status = GrantStatus.Revoked;
                break;
            case GrantExpiredEvent:
                Status = GrantStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw GrantErrors.MissingId();

        if (Subject == default)
            throw GrantErrors.MissingSubject();

        if (Target == default)
            throw GrantErrors.MissingTarget();

        if (!Enum.IsDefined(Status))
            throw GrantErrors.InvalidStateTransition(Status, "validate");
    }
}
