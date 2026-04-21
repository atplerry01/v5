using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Catalog;

namespace Whycespace.Projections.Business.Offering.CatalogCore.Catalog.Reducer;

public static class CatalogProjectionReducer
{
    public static CatalogReadModel Apply(CatalogReadModel state, CatalogCreatedEventSchema e) =>
        state with
        {
            CatalogId = e.AggregateId,
            Name = e.Name,
            Category = e.Category,
            Status = "Draft",
            Members = state.Members
        };

    public static CatalogReadModel Apply(CatalogReadModel state, CatalogMemberAddedEventSchema e) =>
        state with
        {
            CatalogId = e.AggregateId,
            Members = state.Members
                .Concat(new[] { new CatalogMemberReadModel(e.MemberId, e.MemberKind) })
                .ToList()
        };

    public static CatalogReadModel Apply(CatalogReadModel state, CatalogMemberRemovedEventSchema e) =>
        state with
        {
            CatalogId = e.AggregateId,
            Members = state.Members
                .Where(m => !(m.MemberId == e.MemberId && m.MemberKind == e.MemberKind))
                .ToList()
        };

    public static CatalogReadModel Apply(CatalogReadModel state, CatalogPublishedEventSchema e) =>
        state with
        {
            CatalogId = e.AggregateId,
            Status = "Published"
        };

    public static CatalogReadModel Apply(CatalogReadModel state, CatalogArchivedEventSchema e) =>
        state with
        {
            CatalogId = e.AggregateId,
            Status = "Archived"
        };
}
