using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceOption;
using DomainEvents = Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/service/service-core/service-option domain.
///
/// Owns the binding from ServiceOption domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ServiceOptionId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessServiceServiceCoreServiceOptionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ServiceOptionCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOptionCreatedEvent),
            typeof(ServiceOptionCreatedEventSchema));

        sink.RegisterSchema(
            "ServiceOptionUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOptionUpdatedEvent),
            typeof(ServiceOptionUpdatedEventSchema));

        sink.RegisterSchema(
            "ServiceOptionActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOptionActivatedEvent),
            typeof(ServiceOptionActivatedEventSchema));

        sink.RegisterSchema(
            "ServiceOptionArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceOptionArchivedEvent),
            typeof(ServiceOptionArchivedEventSchema));

        sink.RegisterPayloadMapper("ServiceOptionCreatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOptionCreatedEvent)e;
            return new ServiceOptionCreatedEventSchema(
                evt.ServiceOptionId.Value,
                evt.ServiceDefinition.Value,
                evt.Code.Value,
                evt.Name.Value,
                evt.Kind.ToString());
        });
        sink.RegisterPayloadMapper("ServiceOptionUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOptionUpdatedEvent)e;
            return new ServiceOptionUpdatedEventSchema(evt.ServiceOptionId.Value, evt.Name.Value, evt.Kind.ToString());
        });
        sink.RegisterPayloadMapper("ServiceOptionActivatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOptionActivatedEvent)e;
            return new ServiceOptionActivatedEventSchema(evt.ServiceOptionId.Value);
        });
        sink.RegisterPayloadMapper("ServiceOptionArchivedEvent", e =>
        {
            var evt = (DomainEvents.ServiceOptionArchivedEvent)e;
            return new ServiceOptionArchivedEventSchema(evt.ServiceOptionId.Value);
        });
    }
}
