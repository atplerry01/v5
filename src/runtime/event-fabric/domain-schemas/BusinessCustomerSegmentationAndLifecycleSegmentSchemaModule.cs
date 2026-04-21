using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Segment;
using DomainEvents = Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/customer/segmentation-and-lifecycle/segment domain.
///
/// Owns the binding from Segment domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed SegmentId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessCustomerSegmentationAndLifecycleSegmentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "SegmentCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SegmentCreatedEvent),
            typeof(SegmentCreatedEventSchema));

        sink.RegisterSchema(
            "SegmentUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SegmentUpdatedEvent),
            typeof(SegmentUpdatedEventSchema));

        sink.RegisterSchema(
            "SegmentActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SegmentActivatedEvent),
            typeof(SegmentActivatedEventSchema));

        sink.RegisterSchema(
            "SegmentArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SegmentArchivedEvent),
            typeof(SegmentArchivedEventSchema));

        sink.RegisterPayloadMapper("SegmentCreatedEvent", e =>
        {
            var evt = (DomainEvents.SegmentCreatedEvent)e;
            return new SegmentCreatedEventSchema(
                evt.SegmentId.Value,
                evt.Code.Value,
                evt.Name.Value,
                evt.Type.ToString(),
                evt.Criteria.Value);
        });
        sink.RegisterPayloadMapper("SegmentUpdatedEvent", e =>
        {
            var evt = (DomainEvents.SegmentUpdatedEvent)e;
            return new SegmentUpdatedEventSchema(
                evt.SegmentId.Value,
                evt.Name.Value,
                evt.Criteria.Value);
        });
        sink.RegisterPayloadMapper("SegmentActivatedEvent", e =>
        {
            var evt = (DomainEvents.SegmentActivatedEvent)e;
            return new SegmentActivatedEventSchema(evt.SegmentId.Value);
        });
        sink.RegisterPayloadMapper("SegmentArchivedEvent", e =>
        {
            var evt = (DomainEvents.SegmentArchivedEvent)e;
            return new SegmentArchivedEventSchema(evt.SegmentId.Value);
        });
    }
}
