using Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Configuration.ConfigurationResolution;

public sealed class ConfigurationResolutionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ConfigurationResolutionId NewId(string seed) =>
        new(Hex64($"ConfigResolutionTests:{seed}:resolution"));

    [Fact]
    public void Record_RaisesConfigurationResolvedEvent()
    {
        var id = NewId("Record");

        var aggregate = ConfigurationResolutionAggregate.Record(id, "def-1", "scope-1", "state-1", "debug", BaseTime);

        var evt = Assert.IsType<ConfigurationResolvedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("debug", evt.ResolvedValue);
        Assert.Equal(BaseTime, evt.ResolvedAt);
    }

    [Fact]
    public void Record_SetsAllProperties()
    {
        var id = NewId("State");

        var aggregate = ConfigurationResolutionAggregate.Record(id, "def-1", "scope-1", "state-1", "info", BaseTime);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("def-1", aggregate.DefinitionId);
        Assert.Equal("scope-1", aggregate.ScopeId);
        Assert.Equal("state-1", aggregate.StateId);
        Assert.Equal("info", aggregate.ResolvedValue);
    }

    [Fact]
    public void Record_WithEmptyDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationResolutionAggregate.Record(NewId("EmptyDef"), "", "scope", "state", "val", BaseTime));
    }

    [Fact]
    public void Record_WithEmptyScopeId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationResolutionAggregate.Record(NewId("EmptyScope"), "def", "", "state", "val", BaseTime));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new ConfigurationResolvedEvent(id, "def", "scope", "state", "resolved-val", BaseTime) };
        var aggregate = (ConfigurationResolutionAggregate)Activator.CreateInstance(typeof(ConfigurationResolutionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("resolved-val", aggregate.ResolvedValue);
        Assert.Empty(aggregate.DomainEvents);
    }
}
