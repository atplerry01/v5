using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public sealed class ModuleAggregate : AggregateRoot
{
    private static readonly ModuleSpecification Spec = new();

    public ModuleId ModuleId { get; private set; }
    public string CourseRef { get; private set; } = string.Empty;
    public ModuleTitle Title { get; private set; } = default!;
    public int Order { get; private set; }
    public ModuleStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private ModuleAggregate() { }

    public static ModuleAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ModuleId id, string courseRef, ModuleTitle title, int order, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(courseRef)) throw ModuleErrors.InvalidCourseRef();
        if (order < 0) throw ModuleErrors.InvalidOrder();
        var agg = new ModuleAggregate();
        agg.RaiseDomainEvent(new ModuleCreatedEvent(eventId, aggregateId, correlationId, causationId, id, courseRef, title.Value, order, at));
        return agg;
    }

    public void Reorder(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, int newOrder, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (newOrder < 0) throw ModuleErrors.InvalidOrder();
        RaiseDomainEvent(new ModuleReorderedEvent(eventId, aggregateId, correlationId, causationId, ModuleId, newOrder, at));
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == ModuleStatus.Published) throw ModuleErrors.AlreadyPublished();
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new ModulePublishedEvent(eventId, aggregateId, correlationId, causationId, ModuleId, at));
    }

    public void Archive(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == ModuleStatus.Archived) throw ModuleErrors.AlreadyArchived();
        RaiseDomainEvent(new ModuleArchivedEvent(eventId, aggregateId, correlationId, causationId, ModuleId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ModuleCreatedEvent e:
                ModuleId = e.ModuleId;
                CourseRef = e.CourseRef;
                Title = ModuleTitle.Create(e.Title);
                Order = e.Order;
                Status = ModuleStatus.Draft;
                CreatedAt = e.CreatedAt;
                break;
            case ModuleReorderedEvent e: Order = e.Order; break;
            case ModulePublishedEvent: Status = ModuleStatus.Published; break;
            case ModuleArchivedEvent: Status = ModuleStatus.Archived; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Title is null) return;
        if (string.IsNullOrEmpty(CourseRef)) throw ModuleErrors.CourseMissing();
    }
}
