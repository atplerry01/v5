using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceConstraint;
using DomainEvents = Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/service/service-constraint/service-constraint domain.
/// </summary>
public sealed class BusinessServiceServiceConstraintServiceConstraintSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ServiceConstraintCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceConstraintCreatedEvent),
            typeof(ServiceConstraintCreatedEventSchema));

        sink.RegisterSchema(
            "ServiceConstraintUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceConstraintUpdatedEvent),
            typeof(ServiceConstraintUpdatedEventSchema));

        sink.RegisterSchema(
            "ServiceConstraintActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceConstraintActivatedEvent),
            typeof(ServiceConstraintActivatedEventSchema));

        sink.RegisterSchema(
            "ServiceConstraintArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ServiceConstraintArchivedEvent),
            typeof(ServiceConstraintArchivedEventSchema));

        sink.RegisterPayloadMapper("ServiceConstraintCreatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceConstraintCreatedEvent)e;
            return new ServiceConstraintCreatedEventSchema(
                evt.ServiceConstraintId.Value,
                evt.ServiceDefinition.Value,
                (int)evt.Kind,
                evt.Descriptor.Value);
        });
        sink.RegisterPayloadMapper("ServiceConstraintUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceConstraintUpdatedEvent)e;
            return new ServiceConstraintUpdatedEventSchema(
                evt.ServiceConstraintId.Value,
                (int)evt.Kind,
                evt.Descriptor.Value);
        });
        sink.RegisterPayloadMapper("ServiceConstraintActivatedEvent", e =>
        {
            var evt = (DomainEvents.ServiceConstraintActivatedEvent)e;
            return new ServiceConstraintActivatedEventSchema(evt.ServiceConstraintId.Value);
        });
        sink.RegisterPayloadMapper("ServiceConstraintArchivedEvent", e =>
        {
            var evt = (DomainEvents.ServiceConstraintArchivedEvent)e;
            return new ServiceConstraintArchivedEventSchema(evt.ServiceConstraintId.Value);
        });
    }
}
