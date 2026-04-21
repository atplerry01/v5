namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;

public sealed record BundleReadModel
{
    public Guid BundleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<BundleMemberReadModel> Members { get; init; } = Array.Empty<BundleMemberReadModel>();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

public sealed record BundleMemberReadModel(Guid MemberId, string MemberKind);
