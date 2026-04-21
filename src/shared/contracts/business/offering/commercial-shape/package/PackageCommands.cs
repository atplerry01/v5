using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;

public sealed record CreatePackageCommand(
    Guid PackageId,
    string Code,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}

public sealed record AddPackageMemberCommand(
    Guid PackageId,
    string MemberKind,
    Guid MemberId) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}

public sealed record RemovePackageMemberCommand(
    Guid PackageId,
    string MemberKind,
    Guid MemberId) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}

public sealed record ActivatePackageCommand(Guid PackageId) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}

public sealed record ArchivePackageCommand(Guid PackageId) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}
