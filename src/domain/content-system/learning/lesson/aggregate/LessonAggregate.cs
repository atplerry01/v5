using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed class LessonAggregate : AggregateRoot
{
    private static readonly LessonSpecification Spec = new();

    public LessonId LessonId { get; private set; }
    public string ModuleRef { get; private set; } = string.Empty;
    public LessonBody Body { get; private set; } = default!;
    public LessonStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private LessonAggregate() { }

    public static LessonAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        LessonId id, string moduleRef, LessonBody body, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(moduleRef)) throw LessonErrors.InvalidModuleRef();
        var agg = new LessonAggregate();
        agg.RaiseDomainEvent(new LessonCreatedEvent(eventId, aggregateId, correlationId, causationId, id, moduleRef, body.Value, at));
        return agg;
    }

    public void Update(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, LessonBody body, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new LessonUpdatedEvent(eventId, aggregateId, correlationId, causationId, LessonId, body.Value, at));
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == LessonStatus.Published) throw LessonErrors.AlreadyPublished();
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new LessonPublishedEvent(eventId, aggregateId, correlationId, causationId, LessonId, at));
    }

    public void Archive(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == LessonStatus.Archived) throw LessonErrors.AlreadyArchived();
        RaiseDomainEvent(new LessonArchivedEvent(eventId, aggregateId, correlationId, causationId, LessonId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LessonCreatedEvent e:
                LessonId = e.LessonId;
                ModuleRef = e.ModuleRef;
                Body = LessonBody.Create(e.Body);
                Status = LessonStatus.Draft;
                CreatedAt = e.CreatedAt;
                break;
            case LessonUpdatedEvent e: Body = LessonBody.Create(e.Body); break;
            case LessonPublishedEvent: Status = LessonStatus.Published; break;
            case LessonArchivedEvent: Status = LessonStatus.Archived; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Body is null) return;
        if (string.IsNullOrEmpty(ModuleRef)) throw LessonErrors.ModuleMissing();
    }
}
