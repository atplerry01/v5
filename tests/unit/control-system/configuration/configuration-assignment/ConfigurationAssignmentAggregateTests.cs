using Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Configuration.ConfigurationAssignment;

public sealed class ConfigurationAssignmentAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ConfigurationAssignmentId NewId(string seed) =>
        new(Hex64($"ConfigAssignmentTests:{seed}:assignment"));

    [Fact]
    public void Assign_RaisesConfigurationAssignedEvent()
    {
        var id = NewId("Assign");

        var aggregate = ConfigurationAssignmentAggregate.Assign(id, "def-1", "scope-1", "true");

        var evt = Assert.IsType<ConfigurationAssignedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("def-1", evt.DefinitionId);
        Assert.Equal("true", evt.Value);
    }

    [Fact]
    public void Assign_SetsStatusToActive()
    {
        var aggregate = ConfigurationAssignmentAggregate.Assign(NewId("State"), "def-1", "scope-1", "value");

        Assert.Equal(AssignmentStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Assign_WithEmptyDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationAssignmentAggregate.Assign(NewId("EmptyDef"), "", "scope-1", "value"));
    }

    [Fact]
    public void Assign_WithEmptyScopeId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationAssignmentAggregate.Assign(NewId("EmptyScope"), "def-1", "", "value"));
    }

    [Fact]
    public void Assign_WithEmptyValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationAssignmentAggregate.Assign(NewId("EmptyValue"), "def-1", "scope-1", ""));
    }

    [Fact]
    public void Revoke_RaisesConfigurationAssignmentRevokedEvent()
    {
        var aggregate = ConfigurationAssignmentAggregate.Assign(NewId("Revoke"), "def-1", "scope-1", "value");
        aggregate.ClearDomainEvents();

        aggregate.Revoke();

        Assert.IsType<ConfigurationAssignmentRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AssignmentStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Revoke_AlreadyRevoked_Throws()
    {
        var aggregate = ConfigurationAssignmentAggregate.Assign(NewId("DoubleRevoke"), "def-1", "scope-1", "value");
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new ConfigurationAssignedEvent(id, "def-1", "scope-1", "val") };
        var aggregate = (ConfigurationAssignmentAggregate)Activator.CreateInstance(typeof(ConfigurationAssignmentAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AssignmentStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
