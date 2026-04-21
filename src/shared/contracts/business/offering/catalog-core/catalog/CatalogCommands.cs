using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;

public sealed record CreateCatalogCommand(
    Guid CatalogId,
    string Name,
    string Category) : IHasAggregateId
{
    public Guid AggregateId => CatalogId;
}

public sealed record AddCatalogMemberCommand(
    Guid CatalogId,
    Guid MemberId,
    string MemberKind) : IHasAggregateId
{
    public Guid AggregateId => CatalogId;
}

public sealed record RemoveCatalogMemberCommand(
    Guid CatalogId,
    Guid MemberId,
    string MemberKind) : IHasAggregateId
{
    public Guid AggregateId => CatalogId;
}

public sealed record PublishCatalogCommand(Guid CatalogId) : IHasAggregateId
{
    public Guid AggregateId => CatalogId;
}

public sealed record ArchiveCatalogCommand(Guid CatalogId) : IHasAggregateId
{
    public Guid AggregateId => CatalogId;
}
