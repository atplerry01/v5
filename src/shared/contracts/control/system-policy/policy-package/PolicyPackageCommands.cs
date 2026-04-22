using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;

public sealed record AssemblePolicyPackageCommand(
    Guid PackageId,
    string Name,
    int VersionMajor,
    int VersionMinor,
    IReadOnlyList<string> PolicyDefinitionIds) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}

public sealed record DeployPolicyPackageCommand(
    Guid PackageId) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}

public sealed record RetirePolicyPackageCommand(
    Guid PackageId) : IHasAggregateId
{
    public Guid AggregateId => PackageId;
}
