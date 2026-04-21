namespace Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Bundle;

public sealed record BundleCreatedEventSchema(Guid AggregateId, string Name);

// BundleMember VO (Kind + MemberId) is flattened onto the event schema so the
// projection layer doesn't depend on the domain value-object type. MemberKind is
// transported as the enum name (stringified) to keep the wire schema stable.
public sealed record BundleMemberAddedEventSchema(
    Guid AggregateId,
    Guid MemberId,
    string MemberKind);

public sealed record BundleMemberRemovedEventSchema(
    Guid AggregateId,
    Guid MemberId,
    string MemberKind);

public sealed record BundleActivatedEventSchema(Guid AggregateId);

public sealed record BundleArchivedEventSchema(Guid AggregateId);
