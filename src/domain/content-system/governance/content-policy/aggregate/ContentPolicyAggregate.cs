using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public sealed class ContentPolicyAggregate : AggregateRoot
{
    private static readonly ContentPolicySpecification Spec = new();

    public ContentPolicyId PolicyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PolicyRevision CurrentRevision { get; private set; } = default!;
    public ContentPolicyStatus Status { get; private set; }
    public Timestamp DraftedAt { get; private set; }

    private ContentPolicyAggregate() { }

    public static ContentPolicyAggregate Draft(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ContentPolicyId id, string name, PolicyRevision initial, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(name)) throw ContentPolicyErrors.InvalidName();
        var agg = new ContentPolicyAggregate();
        agg.RaiseDomainEvent(new ContentPolicyDraftedEvent(
            eventId, aggregateId, correlationId, causationId, id, name.Trim(), initial.Number, initial.Body, at));
        return agg;
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == ContentPolicyStatus.Published) throw ContentPolicyErrors.AlreadyPublished();
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new ContentPolicyPublishedEvent(eventId, aggregateId, correlationId, causationId, PolicyId, at));
    }

    public void Amend(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, PolicyRevision nextRevision, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        Spec.EnsureAmendmentRevision(CurrentRevision.Number, nextRevision.Number);
        RaiseDomainEvent(new ContentPolicyAmendedEvent(
            eventId, aggregateId, correlationId, causationId, PolicyId, nextRevision.Number, nextRevision.Body, at));
    }

    public void Retire(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == ContentPolicyStatus.Retired) throw ContentPolicyErrors.AlreadyRetired();
        RaiseDomainEvent(new ContentPolicyRetiredEvent(eventId, aggregateId, correlationId, causationId, PolicyId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ContentPolicyDraftedEvent e:
                PolicyId = e.PolicyId;
                Name = e.Name;
                CurrentRevision = PolicyRevision.Create(e.InitialRevision, e.Body);
                Status = ContentPolicyStatus.Draft;
                DraftedAt = e.DraftedAt;
                break;
            case ContentPolicyPublishedEvent: Status = ContentPolicyStatus.Published; break;
            case ContentPolicyAmendedEvent e:
                CurrentRevision = PolicyRevision.Create(e.Revision, e.Body);
                break;
            case ContentPolicyRetiredEvent: Status = ContentPolicyStatus.Retired; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(Name))
            throw ContentPolicyErrors.NameMissing();
    }
}
