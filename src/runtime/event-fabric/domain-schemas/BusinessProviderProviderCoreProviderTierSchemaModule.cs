using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderTier;
using DomainEvents = Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/provider/provider-core/provider-tier domain.
/// </summary>
public sealed class BusinessProviderProviderCoreProviderTierSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProviderTierCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderTierCreatedEvent),
            typeof(ProviderTierCreatedEventSchema));

        sink.RegisterSchema(
            "ProviderTierUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderTierUpdatedEvent),
            typeof(ProviderTierUpdatedEventSchema));

        sink.RegisterSchema(
            "ProviderTierActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderTierActivatedEvent),
            typeof(ProviderTierActivatedEventSchema));

        sink.RegisterSchema(
            "ProviderTierArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderTierArchivedEvent),
            typeof(ProviderTierArchivedEventSchema));

        sink.RegisterPayloadMapper("ProviderTierCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderTierCreatedEvent)e;
            return new ProviderTierCreatedEventSchema(
                evt.ProviderTierId.Value,
                evt.Code.Value,
                evt.Name.Value,
                evt.Rank.Value);
        });
        sink.RegisterPayloadMapper("ProviderTierUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderTierUpdatedEvent)e;
            return new ProviderTierUpdatedEventSchema(evt.ProviderTierId.Value, evt.Name.Value, evt.Rank.Value);
        });
        sink.RegisterPayloadMapper("ProviderTierActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderTierActivatedEvent)e;
            return new ProviderTierActivatedEventSchema(evt.ProviderTierId.Value);
        });
        sink.RegisterPayloadMapper("ProviderTierArchivedEvent", e =>
        {
            var evt = (DomainEvents.ProviderTierArchivedEvent)e;
            return new ProviderTierArchivedEventSchema(evt.ProviderTierId.Value);
        });
    }
}
