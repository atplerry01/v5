using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public sealed class CourseAggregate : AggregateRoot
{
    private static readonly CourseSpecification Spec = new();
    private readonly Dictionary<string, CourseOutlineItem> _outline = new(StringComparer.Ordinal);

    public CourseId CourseId { get; private set; }
    public string OwnerRef { get; private set; } = string.Empty;
    public CourseTitle Title { get; private set; } = default!;
    public CourseStatus Status { get; private set; }
    public Timestamp DraftedAt { get; private set; }
    public IReadOnlyCollection<CourseOutlineItem> Outline => _outline.Values;

    private CourseAggregate() { }

    public static CourseAggregate Draft(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        CourseId id, string ownerRef, CourseTitle title, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(ownerRef)) throw CourseErrors.InvalidOwner();
        var agg = new CourseAggregate();
        agg.RaiseDomainEvent(new CourseDraftedEvent(eventId, aggregateId, correlationId, causationId, id, ownerRef, title.Value, at));
        return agg;
    }

    public void AttachModule(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string moduleRef, int order, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (string.IsNullOrWhiteSpace(moduleRef)) throw CourseErrors.InvalidModuleRef();
        if (_outline.ContainsKey(moduleRef)) throw CourseErrors.ModuleAlreadyAttached(moduleRef);
        RaiseDomainEvent(new CourseModuleAttachedEvent(eventId, aggregateId, correlationId, causationId, CourseId, moduleRef, order, at));
    }

    public void DetachModule(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string moduleRef, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (!_outline.ContainsKey(moduleRef)) throw CourseErrors.ModuleNotAttached(moduleRef);
        RaiseDomainEvent(new CourseModuleDetachedEvent(eventId, aggregateId, correlationId, causationId, CourseId, moduleRef, at));
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsurePublishable(Status, _outline.Count);
        RaiseDomainEvent(new CoursePublishedEvent(eventId, aggregateId, correlationId, causationId, CourseId, at));
    }

    public void Archive(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == CourseStatus.Archived) throw CourseErrors.AlreadyArchived();
        RaiseDomainEvent(new CourseArchivedEvent(eventId, aggregateId, correlationId, causationId, CourseId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CourseDraftedEvent e:
                CourseId = e.CourseId;
                OwnerRef = e.OwnerRef;
                Title = CourseTitle.Create(e.Title);
                Status = CourseStatus.Draft;
                DraftedAt = e.DraftedAt;
                break;
            case CourseModuleAttachedEvent e:
                _outline[e.ModuleRef] = CourseOutlineItem.Attach(e.ModuleRef, e.Order, e.AttachedAt);
                break;
            case CourseModuleDetachedEvent e:
                _outline.Remove(e.ModuleRef);
                break;
            case CoursePublishedEvent: Status = CourseStatus.Published; break;
            case CourseArchivedEvent: Status = CourseStatus.Archived; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Title is null) return;
        if (string.IsNullOrEmpty(OwnerRef)) throw CourseErrors.OwnerMissing();
    }
}
