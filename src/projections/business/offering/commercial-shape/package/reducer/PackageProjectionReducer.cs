using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Package;

namespace Whycespace.Projections.Business.Offering.CommercialShape.Package.Reducer;

public static class PackageProjectionReducer
{
    public static PackageReadModel Apply(PackageReadModel state, PackageCreatedEventSchema e) =>
        state with
        {
            PackageId = e.AggregateId,
            Code = e.Code,
            Name = e.Name,
            Status = "Draft",
            Members = state.Members
        };

    public static PackageReadModel Apply(PackageReadModel state, PackageMemberAddedEventSchema e) =>
        state with
        {
            PackageId = e.AggregateId,
            Members = state.Members
                .Concat(new[] { new PackageMemberReadModel(e.MemberKind, e.MemberId) })
                .ToList()
        };

    public static PackageReadModel Apply(PackageReadModel state, PackageMemberRemovedEventSchema e) =>
        state with
        {
            PackageId = e.AggregateId,
            Members = state.Members
                .Where(m => !(m.MemberKind == e.MemberKind && m.MemberId == e.MemberId))
                .ToList()
        };

    public static PackageReadModel Apply(PackageReadModel state, PackageActivatedEventSchema e) =>
        state with
        {
            PackageId = e.AggregateId,
            Status = "Active"
        };

    public static PackageReadModel Apply(PackageReadModel state, PackageArchivedEventSchema e) =>
        state with
        {
            PackageId = e.AggregateId,
            Status = "Archived"
        };
}
