using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Catalog;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/catalog-core/catalog domain.
///
/// Owns the binding from Catalog domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed CatalogId + CatalogStructure /
/// CatalogMember value-objects) into the shared schema records (Guid AggregateId
/// + flattened primitives) consumed by the projection layer. MemberKind is
/// serialized as the enum name so the wire schema stays stable across replay.
/// </summary>
public sealed class BusinessOfferingCatalogCoreCatalogSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "CatalogCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CatalogCreatedEvent),
            typeof(CatalogCreatedEventSchema));

        sink.RegisterSchema(
            "CatalogMemberAddedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CatalogMemberAddedEvent),
            typeof(CatalogMemberAddedEventSchema));

        sink.RegisterSchema(
            "CatalogMemberRemovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CatalogMemberRemovedEvent),
            typeof(CatalogMemberRemovedEventSchema));

        sink.RegisterSchema(
            "CatalogPublishedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CatalogPublishedEvent),
            typeof(CatalogPublishedEventSchema));

        sink.RegisterSchema(
            "CatalogArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CatalogArchivedEvent),
            typeof(CatalogArchivedEventSchema));

        sink.RegisterPayloadMapper("CatalogCreatedEvent", e =>
        {
            var evt = (DomainEvents.CatalogCreatedEvent)e;
            return new CatalogCreatedEventSchema(
                evt.CatalogId.Value,
                evt.Structure.Name,
                evt.Structure.Category);
        });
        sink.RegisterPayloadMapper("CatalogMemberAddedEvent", e =>
        {
            var evt = (DomainEvents.CatalogMemberAddedEvent)e;
            return new CatalogMemberAddedEventSchema(
                evt.CatalogId.Value,
                evt.Member.MemberId.Value,
                evt.Member.Kind.ToString());
        });
        sink.RegisterPayloadMapper("CatalogMemberRemovedEvent", e =>
        {
            var evt = (DomainEvents.CatalogMemberRemovedEvent)e;
            return new CatalogMemberRemovedEventSchema(
                evt.CatalogId.Value,
                evt.Member.MemberId.Value,
                evt.Member.Kind.ToString());
        });
        sink.RegisterPayloadMapper("CatalogPublishedEvent", e =>
        {
            var evt = (DomainEvents.CatalogPublishedEvent)e;
            return new CatalogPublishedEventSchema(evt.CatalogId.Value);
        });
        sink.RegisterPayloadMapper("CatalogArchivedEvent", e =>
        {
            var evt = (DomainEvents.CatalogArchivedEvent)e;
            return new CatalogArchivedEventSchema(evt.CatalogId.Value);
        });
    }
}
