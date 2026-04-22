using Whycespace.Shared.Contracts.Events.Platform.Schema.Contract;
using DomainEvents = Whycespace.Domain.PlatformSystem.Schema.Contract;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformContractSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ContractRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.ContractRegisteredEvent), typeof(ContractRegisteredEventSchema));
        sink.RegisterSchema("ContractSubscriberAddedEvent", EventVersion.Default,
            typeof(DomainEvents.ContractSubscriberAddedEvent), typeof(ContractSubscriberAddedEventSchema));
        sink.RegisterSchema("ContractDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.ContractDeprecatedEvent), typeof(ContractDeprecatedEventSchema));

        sink.RegisterPayloadMapper("ContractRegisteredEvent", e =>
        {
            var evt = (DomainEvents.ContractRegisteredEvent)e;
            return new ContractRegisteredEventSchema(
                evt.ContractId.Value,
                evt.ContractName,
                evt.PublisherRoute.Classification,
                evt.PublisherRoute.Context,
                evt.PublisherRoute.Domain,
                evt.SchemaRef,
                evt.SchemaVersion);
        });
        sink.RegisterPayloadMapper("ContractSubscriberAddedEvent", e =>
        {
            var evt = (DomainEvents.ContractSubscriberAddedEvent)e;
            return new ContractSubscriberAddedEventSchema(
                evt.ContractId.Value,
                evt.Constraint.SubscriberRoute.Classification,
                evt.Constraint.SubscriberRoute.Context,
                evt.Constraint.SubscriberRoute.Domain,
                evt.Constraint.MinSchemaVersion,
                evt.Constraint.RequiredCompatibilityMode.Value);
        });
        sink.RegisterPayloadMapper("ContractDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.ContractDeprecatedEvent)e;
            return new ContractDeprecatedEventSchema(evt.ContractId.Value);
        });
    }
}
