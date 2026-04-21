using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Configuration;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/commercial-shape/configuration domain.
///
/// Owns the binding from Configuration domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ConfigurationId and ConfigurationOption
/// VO) into the shared schema records (Guid AggregateId + flattened Key/Value pair)
/// consumed by the projection layer.
/// </summary>
public sealed class BusinessOfferingCommercialShapeConfigurationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ConfigurationCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ConfigurationCreatedEvent),
            typeof(ConfigurationCreatedEventSchema));

        sink.RegisterSchema(
            "ConfigurationOptionSetEvent",
            EventVersion.Default,
            typeof(DomainEvents.ConfigurationOptionSetEvent),
            typeof(ConfigurationOptionSetEventSchema));

        sink.RegisterSchema(
            "ConfigurationOptionRemovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ConfigurationOptionRemovedEvent),
            typeof(ConfigurationOptionRemovedEventSchema));

        sink.RegisterSchema(
            "ConfigurationActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ConfigurationActivatedEvent),
            typeof(ConfigurationActivatedEventSchema));

        sink.RegisterSchema(
            "ConfigurationArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ConfigurationArchivedEvent),
            typeof(ConfigurationArchivedEventSchema));

        sink.RegisterPayloadMapper("ConfigurationCreatedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationCreatedEvent)e;
            return new ConfigurationCreatedEventSchema(evt.ConfigurationId.Value, evt.Name.Value);
        });
        sink.RegisterPayloadMapper("ConfigurationOptionSetEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationOptionSetEvent)e;
            return new ConfigurationOptionSetEventSchema(evt.ConfigurationId.Value, evt.Option.Key, evt.Option.Value);
        });
        sink.RegisterPayloadMapper("ConfigurationOptionRemovedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationOptionRemovedEvent)e;
            return new ConfigurationOptionRemovedEventSchema(evt.ConfigurationId.Value, evt.Key);
        });
        sink.RegisterPayloadMapper("ConfigurationActivatedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationActivatedEvent)e;
            return new ConfigurationActivatedEventSchema(evt.ConfigurationId.Value);
        });
        sink.RegisterPayloadMapper("ConfigurationArchivedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationArchivedEvent)e;
            return new ConfigurationArchivedEventSchema(evt.ConfigurationId.Value);
        });
    }
}
