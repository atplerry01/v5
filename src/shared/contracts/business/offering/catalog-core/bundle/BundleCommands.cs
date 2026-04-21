using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;

public sealed record CreateBundleCommand(
    Guid BundleId,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record AddBundleMemberCommand(
    Guid BundleId,
    Guid MemberId,
    string MemberKind) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record RemoveBundleMemberCommand(
    Guid BundleId,
    Guid MemberId,
    string MemberKind) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record ActivateBundleCommand(Guid BundleId) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record ArchiveBundleCommand(Guid BundleId) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}
