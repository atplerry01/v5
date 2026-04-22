using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public sealed class PolicyPackageAggregate : AggregateRoot
{
    public PolicyPackageId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public new PackageVersion Version { get; private set; }
    public IReadOnlySet<string> PolicyDefinitionIds { get; private set; } = new HashSet<string>();
    public PolicyPackageStatus Status { get; private set; }

    private PolicyPackageAggregate() { }

    public static PolicyPackageAggregate Assemble(
        PolicyPackageId id,
        string name,
        PackageVersion version,
        IEnumerable<string> policyDefinitionIds)
    {
        var ids = policyDefinitionIds.ToHashSet();
        Guard.Against(string.IsNullOrEmpty(name), PolicyPackageErrors.PackageNameMustNotBeEmpty().Message);
        Guard.Against(ids.Count == 0, PolicyPackageErrors.PackageMustContainAtLeastOnePolicy().Message);

        var aggregate = new PolicyPackageAggregate();
        aggregate.RaiseDomainEvent(new PolicyPackageAssembledEvent(id, name, version, ids));
        return aggregate;
    }

    public void Deploy()
    {
        Guard.Against(Status == PolicyPackageStatus.Deployed, PolicyPackageErrors.PackageAlreadyDeployed().Message);
        Guard.Against(Status != PolicyPackageStatus.Assembled, PolicyPackageErrors.PackageMustBeAssembledBeforeDeployment().Message);

        RaiseDomainEvent(new PolicyPackageDeployedEvent(Id, Version));
    }

    public void Retire()
    {
        Guard.Against(Status == PolicyPackageStatus.Retired, PolicyPackageErrors.PackageAlreadyRetired().Message);

        RaiseDomainEvent(new PolicyPackageRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PolicyPackageAssembledEvent e:
                Id = e.Id;
                Name = e.Name;
                Version = e.Version;
                PolicyDefinitionIds = e.PolicyDefinitionIds;
                Status = PolicyPackageStatus.Assembled;
                break;
            case PolicyPackageDeployedEvent:
                Status = PolicyPackageStatus.Deployed;
                break;
            case PolicyPackageRetiredEvent:
                Status = PolicyPackageStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "PolicyPackage must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "PolicyPackage must have a non-empty Name.");
        Guard.Against(PolicyDefinitionIds.Count == 0, "PolicyPackage must contain at least one policy definition.");
    }
}
