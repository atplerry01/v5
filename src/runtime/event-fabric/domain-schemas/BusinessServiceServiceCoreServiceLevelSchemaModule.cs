using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceLevel;
using DomainEvents = Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/service/service-core/service-level domain.
///
/// Owns the binding from ServiceLevel domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ServiceLevelId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessServiceServiceCoreServiceLevelSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ServiceLevelCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceLevelCreatedEvent),
            typeof(ServiceLevelCreatedEventSchema));

        sink.RegisterSchema(
            "ServiceLevelUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceLevelUpdatedEvent),
            typeof(ServiceLevelUpdatedEventSchema));

        sink.RegisterSchema(
            "ServiceLevelActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceLevelActivatedEvent),
            typeof(ServiceLevelActivatedEventSchema));

        sink.RegisterSchema(
            "ServiceLevelArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceLevelArchivedEvent),
            typeof(ServiceLevelArchivedEventSchema));

        sink.RegisterPayloadMapper("ServiceLevelCreatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceLevelCreatedEvent)e;
            return new ServiceLevelCreatedEventSchema(
                evt.ServiceLevelId.Value,
                evt.ServiceDefinition.Value,
                evt.Code.Value,
                evt.Name.Value,
                evt.Target.Descriptor);
        });
        sink.RegisterPayloadMapper("ServiceLevelUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceLevelUpdatedEvent)e;
            return new ServiceLevelUpdatedEventSchema(evt.ServiceLevelId.Value, evt.Name.Value, evt.Target.Descriptor);
        });
        sink.RegisterPayloadMapper("ServiceLevelActivatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceLevelActivatedEvent)e;
            return new ServiceLevelActivatedEventSchema(evt.ServiceLevelId.Value);
        });
        sink.RegisterPayloadMapper("ServiceLevelArchivedEvent", e =>
        {
            var evt = (DomainEvents.ServiceLevelArchivedEvent)e;
            return new ServiceLevelArchivedEventSchema(evt.ServiceLevelId.Value);
        });
    }
}
