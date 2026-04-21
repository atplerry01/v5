namespace Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Catalog;

public sealed record CatalogCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Category);

// CatalogMember VO (Kind + MemberId) is flattened onto the event schema so the
// projection layer doesn't depend on the domain value-object type. MemberKind is
// transported as the enum name (stringified) to keep the wire schema stable.
public sealed record CatalogMemberAddedEventSchema(
    Guid AggregateId,
    Guid MemberId,
    string MemberKind);

public sealed record CatalogMemberRemovedEventSchema(
    Guid AggregateId,
    Guid MemberId,
    string MemberKind);

public sealed record CatalogPublishedEventSchema(Guid AggregateId);

public sealed record CatalogArchivedEventSchema(Guid AggregateId);
