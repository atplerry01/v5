using Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Configuration.ConfigurationState;

public sealed class ConfigurationStateAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ConfigurationStateId NewId(string seed) =>
        new(Hex64($"ConfigStateTests:{seed}:state"));

    [Fact]
    public void Set_RaisesConfigurationStateSetEvent()
    {
        var id = NewId("Set");

        var aggregate = ConfigurationStateAggregate.Set(id, "def-1", "debug", 1);

        var evt = Assert.IsType<ConfigurationStateSetEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("debug", evt.Value);
        Assert.Equal(1, evt.Version);
    }

    [Fact]
    public void Set_SetsIsRevokedFalse()
    {
        var aggregate = ConfigurationStateAggregate.Set(NewId("State"), "def-1", "val", 1);

        Assert.False(aggregate.IsRevoked);
    }

    [Fact]
    public void Set_WithEmptyDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationStateAggregate.Set(NewId("EmptyDef"), "", "val", 1));
    }

    [Fact]
    public void Set_WithVersionBelowOne_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationStateAggregate.Set(NewId("BadVersion"), "def-1", "val", 0));
    }

    [Fact]
    public void Revoke_RaisesConfigurationStateRevokedEvent()
    {
        var aggregate = ConfigurationStateAggregate.Set(NewId("Revoke"), "def-1", "val", 1);
        aggregate.ClearDomainEvents();

        aggregate.Revoke();

        Assert.IsType<ConfigurationStateRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsRevoked);
    }

    [Fact]
    public void Revoke_AlreadyRevoked_Throws()
    {
        var aggregate = ConfigurationStateAggregate.Set(NewId("DoubleRevoke"), "def-1", "val", 1);
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new ConfigurationStateSetEvent(id, "def-1", "info", 3) };
        var aggregate = (ConfigurationStateAggregate)Activator.CreateInstance(typeof(ConfigurationStateAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("info", aggregate.Value);
        Assert.Equal(3, aggregate.Version);
        Assert.Empty(aggregate.DomainEvents);
    }
}
