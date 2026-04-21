namespace Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Package;

public sealed record PackageCreatedEventSchema(Guid AggregateId, string Code, string Name);

public sealed record PackageMemberAddedEventSchema(Guid AggregateId, string MemberKind, Guid MemberId);

public sealed record PackageMemberRemovedEventSchema(Guid AggregateId, string MemberKind, Guid MemberId);

public sealed record PackageActivatedEventSchema(Guid AggregateId);

public sealed record PackageArchivedEventSchema(Guid AggregateId);
