using Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Schema;

public sealed class SchemaDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static readonly IReadOnlyList<FieldDescriptor> SomeFields =
    [
        new FieldDescriptor("id", FieldType.String, required: true, null, null),
        new FieldDescriptor("amount", FieldType.Long, required: true, null, null)
    ];

    private static SchemaDefinitionAggregate NewDrafted(string seed)
    {
        var id = new SchemaDefinitionId(IdGen.Generate($"SchemaDefinitionAggregateTests:{seed}"));
        return SchemaDefinitionAggregate.Draft(id, new SchemaName("PaymentSchema"), 1, SomeFields, SchemaCompatibilityMode.Backward, Now);
    }

    [Fact]
    public void Draft_WithValidArgs_RaisesSchemaDefinitionDraftedEvent()
    {
        var aggregate = NewDrafted("Draft");

        Assert.Equal(SchemaStatus.Draft, aggregate.Status);
        Assert.Equal("PaymentSchema", aggregate.SchemaName.Value);
        Assert.Equal(1, aggregate.SchemaVersion);

        var evt = Assert.IsType<SchemaDefinitionDraftedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("Backward", evt.CompatibilityMode.Value);
        Assert.Equal(2, evt.Fields.Count);
    }

    [Fact]
    public void Publish_FromDraft_TransitionsToPublished()
    {
        var aggregate = NewDrafted("Publish");
        aggregate.ClearDomainEvents();

        aggregate.Publish(Now);

        Assert.Equal(SchemaStatus.Published, aggregate.Status);
        Assert.IsType<SchemaDefinitionPublishedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deprecate_FromPublished_TransitionsToDeprecated()
    {
        var aggregate = NewDrafted("DeprecateFromPublished");
        aggregate.Publish(Now);
        aggregate.ClearDomainEvents();

        aggregate.Deprecate(Now);

        Assert.Equal(SchemaStatus.Deprecated, aggregate.Status);
        Assert.IsType<SchemaDefinitionDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deprecate_FromDraft_TransitionsToDeprecated()
    {
        var aggregate = NewDrafted("DeprecateFromDraft");
        aggregate.ClearDomainEvents();

        aggregate.Deprecate(Now);

        Assert.Equal(SchemaStatus.Deprecated, aggregate.Status);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_Throws()
    {
        var aggregate = NewDrafted("DoublePublish");
        aggregate.Publish(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Publish(Now));
    }

    [Fact]
    public void Publish_WhenDeprecated_Throws()
    {
        var aggregate = NewDrafted("PublishFromDeprecated");
        aggregate.Deprecate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Publish(Now));
    }

    [Fact]
    public void Deprecate_WhenAlreadyDeprecated_Throws()
    {
        var aggregate = NewDrafted("DoubleDeprecate");
        aggregate.Deprecate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deprecate(Now));
    }

    [Fact]
    public void Draft_WithNoFields_Throws()
    {
        var id = new SchemaDefinitionId(IdGen.Generate("SchemaDefinitionAggregateTests:NoFields"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            SchemaDefinitionAggregate.Draft(id, new SchemaName("EmptySchema"), 1, [], SchemaCompatibilityMode.None, Now));
    }
}
