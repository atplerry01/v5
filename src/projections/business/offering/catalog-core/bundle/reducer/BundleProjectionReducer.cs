using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Bundle;

namespace Whycespace.Projections.Business.Offering.CatalogCore.Bundle.Reducer;

public static class BundleProjectionReducer
{
    public static BundleReadModel Apply(BundleReadModel state, BundleCreatedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Name = e.Name,
            Status = "Draft",
            Members = state.Members
        };

    public static BundleReadModel Apply(BundleReadModel state, BundleMemberAddedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Members = state.Members
                .Concat(new[] { new BundleMemberReadModel(e.MemberId, e.MemberKind) })
                .ToList()
        };

    public static BundleReadModel Apply(BundleReadModel state, BundleMemberRemovedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Members = state.Members
                .Where(m => !(m.MemberId == e.MemberId && m.MemberKind == e.MemberKind))
                .ToList()
        };

    public static BundleReadModel Apply(BundleReadModel state, BundleActivatedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Status = "Active"
        };

    public static BundleReadModel Apply(BundleReadModel state, BundleArchivedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Status = "Archived"
        };
}
