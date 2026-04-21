using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceWindow;
using DomainEvents = Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/service/service-constraint/service-window domain.
/// </summary>
public sealed class BusinessServiceServiceConstraintServiceWindowSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ServiceWindowCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceWindowCreatedEvent),
            typeof(ServiceWindowCreatedEventSchema));

        sink.RegisterSchema(
            "ServiceWindowUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceWindowUpdatedEvent),
            typeof(ServiceWindowUpdatedEventSchema));

        sink.RegisterSchema(
            "ServiceWindowActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceWindowActivatedEvent),
            typeof(ServiceWindowActivatedEventSchema));

        sink.RegisterSchema(
            "ServiceWindowArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceWindowArchivedEvent),
            typeof(ServiceWindowArchivedEventSchema));

        sink.RegisterPayloadMapper("ServiceWindowCreatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceWindowCreatedEvent)e;
            return new ServiceWindowCreatedEventSchema(
                evt.ServiceWindowId.Value,
                evt.ServiceDefinition.Value,
                evt.Range.StartsAt,
                evt.Range.EndsAt);
        });
        sink.RegisterPayloadMapper("ServiceWindowUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceWindowUpdatedEvent)e;
            return new ServiceWindowUpdatedEventSchema(
                evt.ServiceWindowId.Value,
                evt.Range.StartsAt,
                evt.Range.EndsAt);
        });
        sink.RegisterPayloadMapper("ServiceWindowActivatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceWindowActivatedEvent)e;
            return new ServiceWindowActivatedEventSchema(evt.ServiceWindowId.Value);
        });
        sink.RegisterPayloadMapper("ServiceWindowArchivedEvent", e =>
        {
            var evt = (DomainEvents.ServiceWindowArchivedEvent)e;
            return new ServiceWindowArchivedEventSchema(evt.ServiceWindowId.Value);
        });
    }
}
