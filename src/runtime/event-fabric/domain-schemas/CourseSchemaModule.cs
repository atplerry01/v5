using Whycespace.Shared.Contracts.Events.Content.Learning.Course;
using DomainEvents = Whycespace.Domain.ContentSystem.Learning.Course;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/learning/course domain. Binds domain event
/// CLR types to <see cref="EventSchemaRegistry"/> and registers payload
/// mappers that project domain events into shared-contract schemas for Kafka.
/// </summary>
public sealed class CourseSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("CourseDraftedEvent", EventVersion.Default,
            typeof(DomainEvents.CourseDraftedEvent), typeof(CourseDraftedEventSchema));
        sink.RegisterSchema("CourseModuleAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.CourseModuleAttachedEvent), typeof(CourseModuleAttachedEventSchema));
        sink.RegisterSchema("CourseModuleDetachedEvent", EventVersion.Default,
            typeof(DomainEvents.CourseModuleDetachedEvent), typeof(CourseModuleDetachedEventSchema));
        sink.RegisterSchema("CoursePublishedEvent", EventVersion.Default,
            typeof(DomainEvents.CoursePublishedEvent), typeof(CoursePublishedEventSchema));
        sink.RegisterSchema("CourseArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.CourseArchivedEvent), typeof(CourseArchivedEventSchema));

        sink.RegisterPayloadMapper("CourseDraftedEvent", e =>
        {
            var evt = (DomainEvents.CourseDraftedEvent)e;
            return new CourseDraftedEventSchema(
                evt.AggregateId.Value, evt.CourseId.Value, evt.OwnerRef, evt.Title, evt.DraftedAt.Value);
        });

        sink.RegisterPayloadMapper("CourseModuleAttachedEvent", e =>
        {
            var evt = (DomainEvents.CourseModuleAttachedEvent)e;
            return new CourseModuleAttachedEventSchema(
                evt.AggregateId.Value, evt.CourseId.Value, evt.ModuleRef, evt.Order, evt.AttachedAt.Value);
        });

        sink.RegisterPayloadMapper("CourseModuleDetachedEvent", e =>
        {
            var evt = (DomainEvents.CourseModuleDetachedEvent)e;
            return new CourseModuleDetachedEventSchema(
                evt.AggregateId.Value, evt.CourseId.Value, evt.ModuleRef, evt.DetachedAt.Value);
        });

        sink.RegisterPayloadMapper("CoursePublishedEvent", e =>
        {
            var evt = (DomainEvents.CoursePublishedEvent)e;
            return new CoursePublishedEventSchema(evt.AggregateId.Value, evt.CourseId.Value, evt.PublishedAt.Value);
        });

        sink.RegisterPayloadMapper("CourseArchivedEvent", e =>
        {
            var evt = (DomainEvents.CourseArchivedEvent)e;
            return new CourseArchivedEventSchema(evt.AggregateId.Value, evt.CourseId.Value, evt.ArchivedAt.Value);
        });
    }
}
