using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Bundle;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/catalog-core/bundle domain.
///
/// Owns the binding from Bundle domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed BundleId + BundleMember VO) into
/// the shared schema records (Guid AggregateId + flattened Member fields)
/// consumed by the projection layer. MemberKind is serialized as the enum name
/// so the wire schema stays stable across replay and the projection layer never
/// depends on the domain BundleMemberKind type.
/// </summary>
public sealed class BusinessOfferingCatalogCoreBundleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "BundleCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.BundleCreatedEvent),
            typeof(BundleCreatedEventSchema));

        sink.RegisterSchema(
            "BundleMemberAddedEvent",
            EventVersion.Default,
            typeof(DomainEvents.BundleMemberAddedEvent),
            typeof(BundleMemberAddedEventSchema));

        sink.RegisterSchema(
            "BundleMemberRemovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.BundleMemberRemovedEvent),
            typeof(BundleMemberRemovedEventSchema));

        sink.RegisterSchema(
            "BundleActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.BundleActivatedEvent),
            typeof(BundleActivatedEventSchema));

        sink.RegisterSchema(
            "BundleArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.BundleArchivedEvent),
            typeof(BundleArchivedEventSchema));

        sink.RegisterPayloadMapper("BundleCreatedEvent", e =>
        {
            var evt = (DomainEvents.BundleCreatedEvent)e;
            return new BundleCreatedEventSchema(evt.BundleId.Value, evt.Name.Value);
        });
        sink.RegisterPayloadMapper("BundleMemberAddedEvent", e =>
        {
            var evt = (DomainEvents.BundleMemberAddedEvent)e;
            return new BundleMemberAddedEventSchema(
                evt.BundleId.Value,
                evt.Member.MemberId.Value,
                evt.Member.Kind.ToString());
        });
        sink.RegisterPayloadMapper("BundleMemberRemovedEvent", e =>
        {
            var evt = (DomainEvents.BundleMemberRemovedEvent)e;
            return new BundleMemberRemovedEventSchema(
                evt.BundleId.Value,
                evt.Member.MemberId.Value,
                evt.Member.Kind.ToString());
        });
        sink.RegisterPayloadMapper("BundleActivatedEvent", e =>
        {
            var evt = (DomainEvents.BundleActivatedEvent)e;
            return new BundleActivatedEventSchema(evt.BundleId.Value);
        });
        sink.RegisterPayloadMapper("BundleArchivedEvent", e =>
        {
            var evt = (DomainEvents.BundleArchivedEvent)e;
            return new BundleArchivedEventSchema(evt.BundleId.Value);
        });
    }
}
