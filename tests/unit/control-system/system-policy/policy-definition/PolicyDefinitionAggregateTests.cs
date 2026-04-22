using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemPolicy.PolicyDefinition;

public sealed class PolicyDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PolicyId NewId(string seed) =>
        new(Hex64($"PolicyDefinitionTests:{seed}:policy"));

    private static PolicyScope DefaultScope() =>
        new("control-system", "read:write");

    [Fact]
    public void Define_RaisesPolicyDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var scope = DefaultScope();

        var aggregate = PolicyDefinitionAggregate.Define(id, "Audit Policy", scope);

        var evt = Assert.IsType<PolicyDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("Audit Policy", evt.Name);
    }

    [Fact]
    public void Define_SetsStatusToPublished()
    {
        var aggregate = PolicyDefinitionAggregate.Define(NewId("State"), "Policy", DefaultScope());

        Assert.Equal(PolicyDefinitionStatus.Published, aggregate.Status);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyDefinitionAggregate.Define(NewId("EmptyName"), "", DefaultScope()));
    }

    [Fact]
    public void Deprecate_RaisesPolicyDeprecatedEvent()
    {
        var aggregate = PolicyDefinitionAggregate.Define(NewId("Deprecate"), "Policy", DefaultScope());
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<PolicyDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PolicyDefinitionStatus.Deprecated, aggregate.Status);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = PolicyDefinitionAggregate.Define(NewId("DoubleDeprecate"), "Policy", DefaultScope());
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var scope = DefaultScope();

        var history = new object[] { new PolicyDefinedEvent(id, "P", scope, 1) };
        var aggregate = (PolicyDefinitionAggregate)Activator.CreateInstance(typeof(PolicyDefinitionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PolicyDefinitionStatus.Published, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
