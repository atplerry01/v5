using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceDefinition;
using DomainEvents = Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/service/service-core/service-definition domain.
///
/// Owns the binding from ServiceDefinition domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ServiceDefinitionId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessServiceServiceCoreServiceDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ServiceDefinitionCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceDefinitionCreatedEvent),
            typeof(ServiceDefinitionCreatedEventSchema));

        sink.RegisterSchema(
            "ServiceDefinitionUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceDefinitionUpdatedEvent),
            typeof(ServiceDefinitionUpdatedEventSchema));

        sink.RegisterSchema(
            "ServiceDefinitionActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceDefinitionActivatedEvent),
            typeof(ServiceDefinitionActivatedEventSchema));

        sink.RegisterSchema(
            "ServiceDefinitionArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceDefinitionArchivedEvent),
            typeof(ServiceDefinitionArchivedEventSchema));

        sink.RegisterPayloadMapper("ServiceDefinitionCreatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceDefinitionCreatedEvent)e;
            return new ServiceDefinitionCreatedEventSchema(evt.ServiceDefinitionId.Value, evt.Name.Value, evt.Category.Value);
        });
        sink.RegisterPayloadMapper("ServiceDefinitionUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceDefinitionUpdatedEvent)e;
            return new ServiceDefinitionUpdatedEventSchema(evt.ServiceDefinitionId.Value, evt.Name.Value, evt.Category.Value);
        });
        sink.RegisterPayloadMapper("ServiceDefinitionActivatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceDefinitionActivatedEvent)e;
            return new ServiceDefinitionActivatedEventSchema(evt.ServiceDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("ServiceDefinitionArchivedEvent", e =>
        {
            var evt = (DomainEvents.ServiceDefinitionArchivedEvent)e;
            return new ServiceDefinitionArchivedEventSchema(evt.ServiceDefinitionId.Value);
        });
    }
}
