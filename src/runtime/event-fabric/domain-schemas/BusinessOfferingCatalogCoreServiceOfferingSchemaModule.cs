using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.ServiceOffering;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/catalog-core/service-offering
/// domain.
///
/// Owns the binding from ServiceOffering domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ServiceOfferingId /
/// ServiceOfferingName / ServiceDefinitionRef / OfferingPackageRef
/// value-objects) into the shared schema records (Guid AggregateId + flattened
/// primitives) consumed by the projection layer.
/// </summary>
public sealed class BusinessOfferingCatalogCoreServiceOfferingSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ServiceOfferingCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOfferingCreatedEvent),
            typeof(ServiceOfferingCreatedEventSchema));

        sink.RegisterSchema(
            "ServiceOfferingUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOfferingUpdatedEvent),
            typeof(ServiceOfferingUpdatedEventSchema));

        sink.RegisterSchema(
            "ServiceOfferingActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOfferingActivatedEvent),
            typeof(ServiceOfferingActivatedEventSchema));

        sink.RegisterSchema(
            "ServiceOfferingArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOfferingArchivedEvent),
            typeof(ServiceOfferingArchivedEventSchema));

        sink.RegisterPayloadMapper("ServiceOfferingCreatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOfferingCreatedEvent)e;
            return new ServiceOfferingCreatedEventSchema(
                evt.ServiceOfferingId.Value,
                evt.Name.Value,
                evt.ServiceDefinition.Value,
                evt.Package?.Value);
        });
        sink.RegisterPayloadMapper("ServiceOfferingUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOfferingUpdatedEvent)e;
            return new ServiceOfferingUpdatedEventSchema(
                evt.ServiceOfferingId.Value,
                evt.Name.Value);
        });
        sink.RegisterPayloadMapper("ServiceOfferingActivatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOfferingActivatedEvent)e;
            return new ServiceOfferingActivatedEventSchema(evt.ServiceOfferingId.Value);
        });
        sink.RegisterPayloadMapper("ServiceOfferingArchivedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOfferingArchivedEvent)e;
            return new ServiceOfferingArchivedEventSchema(evt.ServiceOfferingId.Value);
        });
    }
}
