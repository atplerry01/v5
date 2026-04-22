using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyPackage;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyPackage.Reducer;

public static class PolicyPackageProjectionReducer
{
    public static PolicyPackageReadModel Apply(PolicyPackageReadModel state, PolicyPackageAssembledEventSchema e) =>
        state with
        {
            PackageId           = e.AggregateId,
            Name                = e.Name,
            VersionMajor        = e.VersionMajor,
            VersionMinor        = e.VersionMinor,
            PolicyDefinitionIds = e.PolicyDefinitionIds,
            Status              = "Assembled"
        };

    public static PolicyPackageReadModel Apply(PolicyPackageReadModel state, PolicyPackageDeployedEventSchema e) =>
        state with
        {
            VersionMajor = e.VersionMajor,
            VersionMinor = e.VersionMinor,
            Status       = "Deployed"
        };

    public static PolicyPackageReadModel Apply(PolicyPackageReadModel state, PolicyPackageRetiredEventSchema e) =>
        state with { Status = "Retired" };
}
