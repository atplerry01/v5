namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public abstract class AggregateRoot
{
    private readonly List<object> _domainEvents = new();
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();
    public int Version { get; private set; } = -1;

    // AGGREGATE-IDENTITY-REHYDRATION-01: canonical aggregate id stamped by the
    // reconstruction loader BEFORE LoadFromHistory runs. This is the identity
    // the event store used to bucket this aggregate's events; it is the
    // ground-truth id regardless of how well the payload JSON round-trips into
    // the typed domain-event record (payload keys may be `AggregateId` while
    // the record exposes a typed value-object property like `FxId` — such a
    // name mismatch defaults the value object to its empty form on replay).
    // Apply methods in concrete aggregates MUST fall back to this value when
    // the event-carried identity is empty, so the aggregate's own identity
    // field is always populated post-replay. Subsequent mutating events emitted
    // by the aggregate then carry the correct aggregate id end-to-end
    // (events row → outbox row → Kafka key → aggregate-id header).
    protected Guid AggregateIdentity { get; private set; }

    /// <summary>
    /// Called by the aggregate-reconstruction loader (RuntimeCommandDispatcher)
    /// BEFORE <see cref="LoadFromHistory"/>. Stamps the canonical aggregate id
    /// from the event-store column so concrete Apply methods can recover identity
    /// when a stored payload's key shape does not match the domain record.
    /// Idempotent; a non-empty id once set cannot be changed.
    /// </summary>
    public void HydrateIdentity(Guid aggregateId)
    {
        if (aggregateId == Guid.Empty)
            throw new InvalidOperationException(
                "HydrateIdentity requires a non-empty aggregate id (AGGREGATE-IDENTITY-REHYDRATION-01).");
        if (AggregateIdentity != Guid.Empty && AggregateIdentity != aggregateId)
            throw new InvalidOperationException(
                "AggregateIdentity already hydrated to a different id.");
        AggregateIdentity = aggregateId;
    }

    protected void RaiseDomainEvent(object domainEvent)
    {
        ValidateBeforeChange();
        // E1.X/AGGREGATE-ROOT-ORDER: Apply MUST precede EnsureInvariants.
        //
        // Invariants describe the aggregate's state AFTER a transition, not
        // before. Running EnsureInvariants() before Apply() checked the
        // pre-transition state, which on a factory path (where the aggregate
        // is constructed via the parameterless private constructor and the
        // very first RaiseDomainEvent seeds its identity fields) forced the
        // invariant to run against default/zero values and erroneously fail
        // post-construction aggregates before their seed event was applied
        // (e.g. AssetAggregate.Create rejecting itself with "OwnerId must
        // not be empty for a non-disposed asset" even though the event
        // carried a valid OwnerId).
        //
        // Correct sequence: apply the event → state reflects the transition →
        // enforce the invariant against the post-transition state. On
        // violation, the exception propagates out of Apply's caller so the
        // event is NOT added to the pending list and downstream publication
        // is suppressed.
        // POLICY HOOK (to be enforced by runtime)
        Apply(domainEvent);
        EnsureInvariants();
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void LoadFromHistory(IEnumerable<object> history)
    {
        foreach (var e in history)
        {
            Apply(e);
            Version++;
        }
    }

    /// <summary>
    /// Apply an event to mutate aggregate state. Override in derived aggregates.
    /// </summary>
    protected virtual void Apply(object domainEvent) { }

    protected virtual void EnsureInvariants()
    {
        // Override in derived aggregates to enforce domain invariants
    }

    protected virtual void ValidateBeforeChange()
    {
        // Override in derived aggregates for pre-change validation
    }
}
