using Whycespace.Domain.PlatformSystem.Command.CommandDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Command;

public sealed class CommandDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static CommandDefinitionAggregate NewDefined(string seed)
    {
        var id = new CommandDefinitionId(IdGen.Generate($"CommandDefinitionAggregateTests:{seed}"));
        var owner = new DomainRoute("economic", "capital", "account");
        return CommandDefinitionAggregate.Define(id, new CommandTypeName("CreateAccount"), new CommandVersion(1), "schema-001", owner, Now);
    }

    [Fact]
    public void Define_WithValidArgs_RaisesCommandDefinedEvent()
    {
        var aggregate = NewDefined("Define");

        Assert.Equal(CommandDefinitionStatus.Active, aggregate.Status);
        var evt = Assert.IsType<CommandDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("CreateAccount", evt.TypeName.Value);
        Assert.Equal(1, evt.Version.Value);
        Assert.Equal("schema-001", evt.SchemaId);
    }

    [Fact]
    public void Deprecate_FromActive_TransitionsToDeprecated()
    {
        var aggregate = NewDefined("Deprecate");
        aggregate.ClearDomainEvents();

        aggregate.Deprecate(Now);

        Assert.Equal(CommandDefinitionStatus.Deprecated, aggregate.Status);
        Assert.IsType<CommandDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deprecate_WhenAlreadyDeprecated_Throws()
    {
        var aggregate = NewDefined("DoubleDeprecate");
        aggregate.Deprecate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deprecate(Now));
    }

    [Fact]
    public void Define_WithEmptySchemaId_Throws()
    {
        var id = new CommandDefinitionId(IdGen.Generate("CommandDefinitionAggregateTests:EmptySchema"));
        var owner = new DomainRoute("economic", "capital", "account");

        Assert.Throws<DomainInvariantViolationException>(() =>
            CommandDefinitionAggregate.Define(id, new CommandTypeName("CreateAccount"), new CommandVersion(1), "", owner, Now));
    }

    [Fact]
    public void Define_WithInvalidOwnerRoute_Throws()
    {
        var id = new CommandDefinitionId(IdGen.Generate("CommandDefinitionAggregateTests:InvalidRoute"));
        var invalidRoute = new DomainRoute("", "capital", "account");

        Assert.Throws<DomainInvariantViolationException>(() =>
            CommandDefinitionAggregate.Define(id, new CommandTypeName("CreateAccount"), new CommandVersion(1), "schema-001", invalidRoute, Now));
    }
}
