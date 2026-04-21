using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderCapability;
using DomainEvents = Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/provider/provider-core/provider-capability domain.
/// </summary>
public sealed class BusinessProviderProviderCoreProviderCapabilitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProviderCapabilityCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCapabilityCreatedEvent),
            typeof(ProviderCapabilityCreatedEventSchema));

        sink.RegisterSchema(
            "ProviderCapabilityUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCapabilityUpdatedEvent),
            typeof(ProviderCapabilityUpdatedEventSchema));

        sink.RegisterSchema(
            "ProviderCapabilityActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCapabilityActivatedEvent),
            typeof(ProviderCapabilityActivatedEventSchema));

        sink.RegisterSchema(
            "ProviderCapabilityArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCapabilityArchivedEvent),
            typeof(ProviderCapabilityArchivedEventSchema));

        sink.RegisterPayloadMapper("ProviderCapabilityCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCapabilityCreatedEvent)e;
            return new ProviderCapabilityCreatedEventSchema(
                evt.ProviderCapabilityId.Value,
                evt.Provider.Value,
                evt.Code.Value,
                evt.Name.Value);
        });
        sink.RegisterPayloadMapper("ProviderCapabilityUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCapabilityUpdatedEvent)e;
            return new ProviderCapabilityUpdatedEventSchema(evt.ProviderCapabilityId.Value, evt.Name.Value);
        });
        sink.RegisterPayloadMapper("ProviderCapabilityActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCapabilityActivatedEvent)e;
            return new ProviderCapabilityActivatedEventSchema(evt.ProviderCapabilityId.Value);
        });
        sink.RegisterPayloadMapper("ProviderCapabilityArchivedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCapabilityArchivedEvent)e;
            return new ProviderCapabilityArchivedEventSchema(evt.ProviderCapabilityId.Value);
        });
    }
}
