using Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Configuration.ConfigurationDefinition;

public sealed class ConfigurationDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ConfigurationDefinitionId NewId(string seed) =>
        new(Hex64($"ConfigDefinitionTests:{seed}:definition"));

    [Fact]
    public void Define_RaisesConfigurationDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = ConfigurationDefinitionAggregate.Define(id, "log-level", ConfigValueType.String, "Sets log verbosity");

        var evt = Assert.IsType<ConfigurationDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("log-level", evt.Name);
        Assert.Equal(ConfigValueType.String, evt.ValueType);
    }

    [Fact]
    public void Define_SetsIsDeprecatedFalse()
    {
        var aggregate = ConfigurationDefinitionAggregate.Define(NewId("State"), "cfg", ConfigValueType.Boolean, "desc");

        Assert.False(aggregate.IsDeprecated);
        Assert.Null(aggregate.DefaultValue);
    }

    [Fact]
    public void Define_WithDefaultValue_SetsDefaultValue()
    {
        var aggregate = ConfigurationDefinitionAggregate.Define(
            NewId("Default"), "max-retries", ConfigValueType.Integer, "Max retries", "3");

        Assert.Equal("3", aggregate.DefaultValue);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationDefinitionAggregate.Define(NewId("EmptyName"), "", ConfigValueType.String, "desc"));
    }

    [Fact]
    public void Define_WithEmptyDescription_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationDefinitionAggregate.Define(NewId("EmptyDesc"), "name", ConfigValueType.String, ""));
    }

    [Fact]
    public void Deprecate_RaisesConfigurationDefinitionDeprecatedEvent()
    {
        var aggregate = ConfigurationDefinitionAggregate.Define(NewId("Deprecate"), "cfg", ConfigValueType.Json, "desc");
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<ConfigurationDefinitionDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsDeprecated);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = ConfigurationDefinitionAggregate.Define(NewId("DoubleDeprecate"), "cfg", ConfigValueType.String, "desc");
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new ConfigurationDefinedEvent(id, "cfg", ConfigValueType.Integer, "desc", null) };
        var aggregate = (ConfigurationDefinitionAggregate)Activator.CreateInstance(typeof(ConfigurationDefinitionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ConfigValueType.Integer, aggregate.ValueType);
        Assert.Empty(aggregate.DomainEvents);
    }
}
