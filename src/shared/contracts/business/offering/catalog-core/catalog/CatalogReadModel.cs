namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;

public sealed record CatalogReadModel
{
    public Guid CatalogId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<CatalogMemberReadModel> Members { get; init; } = Array.Empty<CatalogMemberReadModel>();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

public sealed record CatalogMemberReadModel(Guid MemberId, string MemberKind);
