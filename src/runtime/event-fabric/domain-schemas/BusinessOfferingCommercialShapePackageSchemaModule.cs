using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Package;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/commercial-shape/package domain.
///
/// Owns the binding from Package domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed PackageId + PackageMember VO) into
/// the shared schema records (Guid AggregateId + flattened MemberKind/MemberId
/// pair) consumed by the projection layer.
/// </summary>
public sealed class BusinessOfferingCommercialShapePackageSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "PackageCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PackageCreatedEvent),
            typeof(PackageCreatedEventSchema));

        sink.RegisterSchema(
            "PackageMemberAddedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PackageMemberAddedEvent),
            typeof(PackageMemberAddedEventSchema));

        sink.RegisterSchema(
            "PackageMemberRemovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PackageMemberRemovedEvent),
            typeof(PackageMemberRemovedEventSchema));

        sink.RegisterSchema(
            "PackageActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PackageActivatedEvent),
            typeof(PackageActivatedEventSchema));

        sink.RegisterSchema(
            "PackageArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PackageArchivedEvent),
            typeof(PackageArchivedEventSchema));

        sink.RegisterPayloadMapper("PackageCreatedEvent", e =>
        {
            var evt = (DomainEvents.PackageCreatedEvent)e;
            return new PackageCreatedEventSchema(evt.PackageId.Value, evt.Code.Value, evt.Name.Value);
        });
        sink.RegisterPayloadMapper("PackageMemberAddedEvent", e =>
        {
            var evt = (DomainEvents.PackageMemberAddedEvent)e;
            return new PackageMemberAddedEventSchema(
                evt.PackageId.Value,
                evt.Member.Kind.ToString(),
                evt.Member.MemberId.Value);
        });
        sink.RegisterPayloadMapper("PackageMemberRemovedEvent", e =>
        {
            var evt = (DomainEvents.PackageMemberRemovedEvent)e;
            return new PackageMemberRemovedEventSchema(
                evt.PackageId.Value,
                evt.Member.Kind.ToString(),
                evt.Member.MemberId.Value);
        });
        sink.RegisterPayloadMapper("PackageActivatedEvent", e =>
        {
            var evt = (DomainEvents.PackageActivatedEvent)e;
            return new PackageActivatedEventSchema(evt.PackageId.Value);
        });
        sink.RegisterPayloadMapper("PackageArchivedEvent", e =>
        {
            var evt = (DomainEvents.PackageArchivedEvent)e;
            return new PackageArchivedEventSchema(evt.PackageId.Value);
        });
    }
}
