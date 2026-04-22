using Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Configuration.ConfigurationScope;

public sealed class ConfigurationScopeAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ConfigurationScopeId NewId(string seed) =>
        new(Hex64($"ConfigScopeTests:{seed}:scope"));

    [Fact]
    public void Declare_RaisesConfigurationScopeDeclaredEvent()
    {
        var id = NewId("Declare");

        var aggregate = ConfigurationScopeAggregate.Declare(id, "def-1", "control-system");

        var evt = Assert.IsType<ConfigurationScopeDeclaredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("def-1", evt.DefinitionId);
        Assert.Equal("control-system", evt.Classification);
        Assert.Null(evt.Context);
    }

    [Fact]
    public void Declare_WithContext_SetsContext()
    {
        var aggregate = ConfigurationScopeAggregate.Declare(NewId("WithContext"), "def-1", "control-system", "audit");

        Assert.Equal("audit", aggregate.Context);
    }

    [Fact]
    public void Declare_SetsIsRemovedFalse()
    {
        var aggregate = ConfigurationScopeAggregate.Declare(NewId("State"), "def-1", "cls");

        Assert.False(aggregate.IsRemoved);
    }

    [Fact]
    public void Declare_WithEmptyDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationScopeAggregate.Declare(NewId("EmptyDef"), "", "cls"));
    }

    [Fact]
    public void Declare_WithEmptyClassification_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConfigurationScopeAggregate.Declare(NewId("EmptyCls"), "def-1", ""));
    }

    [Fact]
    public void Remove_RaisesConfigurationScopeRemovedEvent()
    {
        var aggregate = ConfigurationScopeAggregate.Declare(NewId("Remove"), "def-1", "cls");
        aggregate.ClearDomainEvents();

        aggregate.Remove();

        Assert.IsType<ConfigurationScopeRemovedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsRemoved);
    }

    [Fact]
    public void Remove_AlreadyRemoved_Throws()
    {
        var aggregate = ConfigurationScopeAggregate.Declare(NewId("DoubleRemove"), "def-1", "cls");
        aggregate.Remove();

        Assert.ThrowsAny<Exception>(() => aggregate.Remove());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new ConfigurationScopeDeclaredEvent(id, "def-1", "control-system", "audit") };
        var aggregate = (ConfigurationScopeAggregate)Activator.CreateInstance(typeof(ConfigurationScopeAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("audit", aggregate.Context);
        Assert.Empty(aggregate.DomainEvents);
    }
}
