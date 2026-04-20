namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed class GrantAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public GrantId Id { get; private set; }
    public GrantSubjectRef Subject { get; private set; }
    public GrantTargetRef Target { get; private set; }
    public GrantScope Scope { get; private set; }
    public GrantStatus Status { get; private set; }
    public GrantExpiry? Expiry { get; private set; }
    public int Version { get; private set; }

    private GrantAggregate() { }

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

        var @event = new GrantCreatedEvent(id, subject, target, scope, expiry);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate(DateTimeOffset activatedAt)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw GrantErrors.AlreadyTerminal(Id, Status);

        if (Expiry.HasValue && Expiry.Value.IsExpiredAt(activatedAt))
            throw GrantErrors.ExpiryInPast();

        var @event = new GrantActivatedEvent(Id, activatedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw GrantErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new GrantRevokedEvent(Id, revokedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
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

        var @event = new GrantExpiredEvent(Id, expiredAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(GrantCreatedEvent @event)
    {
        Id = @event.GrantId;
        Subject = @event.Subject;
        Target = @event.Target;
        Scope = @event.Scope;
        Expiry = @event.Expiry;
        Status = GrantStatus.Pending;
        Version++;
    }

    private void Apply(GrantActivatedEvent @event)
    {
        Status = GrantStatus.Active;
        Version++;
    }

    private void Apply(GrantRevokedEvent @event)
    {
        Status = GrantStatus.Revoked;
        Version++;
    }

    private void Apply(GrantExpiredEvent @event)
    {
        Status = GrantStatus.Expired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
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
