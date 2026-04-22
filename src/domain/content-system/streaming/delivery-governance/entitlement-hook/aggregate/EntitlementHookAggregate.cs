using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class EntitlementHookAggregate : AggregateRoot
{
    public EntitlementHookId HookId { get; private set; }
    public EntitlementTargetRef TargetRef { get; private set; }
    public SourceSystemRef SourceSystem { get; private set; }
    public EntitlementStatus Status { get; private set; }
    public Timestamp? LastCheckedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public Timestamp RegisteredAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private EntitlementHookAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static EntitlementHookAggregate Register(
        EntitlementHookId hookId,
        EntitlementTargetRef targetRef,
        SourceSystemRef sourceSystem,
        Timestamp registeredAt)
    {
        var aggregate = new EntitlementHookAggregate();
        aggregate.RaiseDomainEvent(new EntitlementHookRegisteredEvent(hookId, targetRef, sourceSystem, registeredAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void RecordQuery(EntitlementStatus result, Timestamp queriedAt)
    {
        if (result == EntitlementStatus.Unknown || result == EntitlementStatus.Error)
            throw EntitlementHookErrors.InvalidQueryResult();

        RaiseDomainEvent(new EntitlementQueriedEvent(HookId, result, queriedAt));
    }

    public void Refresh(EntitlementStatus result, Timestamp refreshedAt)
    {
        if (result == EntitlementStatus.Unknown || result == EntitlementStatus.Error)
            throw EntitlementHookErrors.InvalidQueryResult();

        RaiseDomainEvent(new EntitlementRefreshedEvent(HookId, result, refreshedAt));
    }

    public void Invalidate(Timestamp invalidatedAt)
    {
        RaiseDomainEvent(new EntitlementInvalidatedEvent(HookId, invalidatedAt));
    }

    public void RecordFailure(string reason, Timestamp failedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw EntitlementHookErrors.EmptyFailureReason();

        RaiseDomainEvent(new EntitlementFailureRecordedEvent(HookId, reason.Trim(), failedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EntitlementHookRegisteredEvent e:
                HookId = e.HookId;
                TargetRef = e.TargetRef;
                SourceSystem = e.SourceSystem;
                Status = EntitlementStatus.Unknown;
                RegisteredAt = e.RegisteredAt;
                LastModifiedAt = e.RegisteredAt;
                break;

            case EntitlementQueriedEvent e:
                Status = e.Result;
                FailureReason = null;
                LastCheckedAt = e.QueriedAt;
                LastModifiedAt = e.QueriedAt;
                break;

            case EntitlementRefreshedEvent e:
                Status = e.Result;
                FailureReason = null;
                LastCheckedAt = e.RefreshedAt;
                LastModifiedAt = e.RefreshedAt;
                break;

            case EntitlementInvalidatedEvent e:
                Status = EntitlementStatus.Unknown;
                LastModifiedAt = e.InvalidatedAt;
                break;

            case EntitlementFailureRecordedEvent e:
                Status = EntitlementStatus.Error;
                FailureReason = e.Reason;
                LastCheckedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (HookId.Value == Guid.Empty)
            throw EntitlementHookErrors.MissingHookId();

        if (TargetRef.Value == Guid.Empty)
            throw EntitlementHookErrors.MissingTargetRef();
    }
}
