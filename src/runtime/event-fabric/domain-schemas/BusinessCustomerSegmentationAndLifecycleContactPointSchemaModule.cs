using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using DomainEvents = Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/customer/segmentation-and-lifecycle/contact-point domain.
///
/// Owns the binding from ContactPoint domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ContactPointId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessCustomerSegmentationAndLifecycleContactPointSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ContactPointCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContactPointCreatedEvent),
            typeof(ContactPointCreatedEventSchema));

        sink.RegisterSchema(
            "ContactPointUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContactPointUpdatedEvent),
            typeof(ContactPointUpdatedEventSchema));

        sink.RegisterSchema(
            "ContactPointActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContactPointActivatedEvent),
            typeof(ContactPointActivatedEventSchema));

        sink.RegisterSchema(
            "ContactPointPreferredSetEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContactPointPreferredSetEvent),
            typeof(ContactPointPreferredSetEventSchema));

        sink.RegisterSchema(
            "ContactPointArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContactPointArchivedEvent),
            typeof(ContactPointArchivedEventSchema));

        sink.RegisterPayloadMapper("ContactPointCreatedEvent", e =>
        {
            var evt = (DomainEvents.ContactPointCreatedEvent)e;
            return new ContactPointCreatedEventSchema(
                evt.ContactPointId.Value,
                evt.Customer.Value,
                evt.Kind.ToString(),
                evt.Value.Value);
        });
        sink.RegisterPayloadMapper("ContactPointUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ContactPointUpdatedEvent)e;
            return new ContactPointUpdatedEventSchema(evt.ContactPointId.Value, evt.Value.Value);
        });
        sink.RegisterPayloadMapper("ContactPointActivatedEvent", e =>
        {
            var evt = (DomainEvents.ContactPointActivatedEvent)e;
            return new ContactPointActivatedEventSchema(evt.ContactPointId.Value);
        });
        sink.RegisterPayloadMapper("ContactPointPreferredSetEvent", e =>
        {
            var evt = (DomainEvents.ContactPointPreferredSetEvent)e;
            return new ContactPointPreferredSetEventSchema(evt.ContactPointId.Value, evt.IsPreferred);
        });
        sink.RegisterPayloadMapper("ContactPointArchivedEvent", e =>
        {
            var evt = (DomainEvents.ContactPointArchivedEvent)e;
            return new ContactPointArchivedEventSchema(evt.ContactPointId.Value);
        });
    }
}
