using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderAvailability;
using DomainEvents = Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/provider/provider-scope/provider-availability domain.
///
/// Owns the binding from ProviderAvailability domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ProviderAvailabilityId) into the
/// shared schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessProviderProviderScopeProviderAvailabilitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProviderAvailabilityCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAvailabilityCreatedEvent),
            typeof(ProviderAvailabilityCreatedEventSchema));

        sink.RegisterSchema(
            "ProviderAvailabilityUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAvailabilityUpdatedEvent),
            typeof(ProviderAvailabilityUpdatedEventSchema));

        sink.RegisterSchema(
            "ProviderAvailabilityActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAvailabilityActivatedEvent),
            typeof(ProviderAvailabilityActivatedEventSchema));

        sink.RegisterSchema(
            "ProviderAvailabilityArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAvailabilityArchivedEvent),
            typeof(ProviderAvailabilityArchivedEventSchema));

        sink.RegisterPayloadMapper("ProviderAvailabilityCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAvailabilityCreatedEvent)e;
            return new ProviderAvailabilityCreatedEventSchema(
                evt.ProviderAvailabilityId.Value,
                evt.Provider.Value,
                evt.Window.StartsAt,
                evt.Window.EndsAt);
        });
        sink.RegisterPayloadMapper("ProviderAvailabilityUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAvailabilityUpdatedEvent)e;
            return new ProviderAvailabilityUpdatedEventSchema(
                evt.ProviderAvailabilityId.Value,
                evt.Window.StartsAt,
                evt.Window.EndsAt);
        });
        sink.RegisterPayloadMapper("ProviderAvailabilityActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAvailabilityActivatedEvent)e;
            return new ProviderAvailabilityActivatedEventSchema(evt.ProviderAvailabilityId.Value);
        });
        sink.RegisterPayloadMapper("ProviderAvailabilityArchivedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAvailabilityArchivedEvent)e;
            return new ProviderAvailabilityArchivedEventSchema(evt.ProviderAvailabilityId.Value);
        });
    }
}
