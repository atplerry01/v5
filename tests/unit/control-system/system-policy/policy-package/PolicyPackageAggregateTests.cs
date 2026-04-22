using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemPolicy.PolicyPackage;

public sealed class PolicyPackageAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PolicyPackageId NewId(string seed) =>
        new(Hex64($"PolicyPackageTests:{seed}:package"));

    private static IEnumerable<string> DefaultPolicyIds(string seed) =>
        [Hex64($"PolicyPackageTests:{seed}:pid1"), Hex64($"PolicyPackageTests:{seed}:pid2")];

    [Fact]
    public void Assemble_RaisesPolicyPackageAssembledEvent()
    {
        var id = NewId("Assemble");

        var aggregate = PolicyPackageAggregate.Assemble(id, "Core Package", new PackageVersion(1, 0), DefaultPolicyIds("Assemble"));

        var evt = Assert.IsType<PolicyPackageAssembledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("Core Package", evt.Name);
        Assert.Equal(2, evt.PolicyDefinitionIds.Count);
    }

    [Fact]
    public void Assemble_SetsStatusToAssembled()
    {
        var aggregate = PolicyPackageAggregate.Assemble(NewId("Status"), "P", new PackageVersion(1, 0), DefaultPolicyIds("Status"));

        Assert.Equal(PolicyPackageStatus.Assembled, aggregate.Status);
    }

    [Fact]
    public void Assemble_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyPackageAggregate.Assemble(NewId("EmptyName"), "", new PackageVersion(1, 0), DefaultPolicyIds("EmptyName")));
    }

    [Fact]
    public void Assemble_WithNoPolicies_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyPackageAggregate.Assemble(NewId("NoPolicies"), "P", new PackageVersion(1, 0), []));
    }

    [Fact]
    public void Deploy_RaisesPolicyPackageDeployedEvent()
    {
        var aggregate = PolicyPackageAggregate.Assemble(NewId("Deploy"), "P", new PackageVersion(1, 0), DefaultPolicyIds("Deploy"));
        aggregate.ClearDomainEvents();

        aggregate.Deploy();

        Assert.IsType<PolicyPackageDeployedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PolicyPackageStatus.Deployed, aggregate.Status);
    }

    [Fact]
    public void Deploy_AlreadyDeployed_Throws()
    {
        var aggregate = PolicyPackageAggregate.Assemble(NewId("DoubleDeployed"), "P", new PackageVersion(1, 0), DefaultPolicyIds("DoubleDeployed"));
        aggregate.Deploy();

        Assert.ThrowsAny<Exception>(() => aggregate.Deploy());
    }

    [Fact]
    public void Retire_RaisesPolicyPackageRetiredEvent()
    {
        var aggregate = PolicyPackageAggregate.Assemble(NewId("Retire"), "P", new PackageVersion(1, 0), DefaultPolicyIds("Retire"));
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<PolicyPackageRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PolicyPackageStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var version = new PackageVersion(2, 1);
        var ids = DefaultPolicyIds("History").ToHashSet();

        var history = new object[] { new PolicyPackageAssembledEvent(id, "P", version, ids) };
        var aggregate = (PolicyPackageAggregate)Activator.CreateInstance(typeof(PolicyPackageAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PolicyPackageStatus.Assembled, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
