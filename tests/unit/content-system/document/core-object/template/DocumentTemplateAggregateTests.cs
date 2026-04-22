using Whycespace.Domain.ContentSystem.Document.CoreObject.Template;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.CoreObject.Template;

public sealed class DocumentTemplateAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentTemplateId NewId(string seed) =>
        new(IdGen.Generate($"DocumentTemplateAggregateTests:{seed}:template"));

    [Fact]
    public void Create_RaisesDocumentTemplateCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var name = new TemplateName("Standard Contract");

        var aggregate = DocumentTemplateAggregate.Create(id, name, TemplateType.Contract, null, BaseTime);

        var evt = Assert.IsType<DocumentTemplateCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TemplateId);
        Assert.Equal(name, evt.Name);
        Assert.Equal(TemplateType.Contract, evt.Type);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");

        var aggregate = DocumentTemplateAggregate.Create(id, new TemplateName("Invoice Tmpl"), TemplateType.Invoice, null, BaseTime);

        Assert.Equal(id, aggregate.TemplateId);
        Assert.Equal(TemplateStatus.Draft, aggregate.Status);
    }

    [Fact]
    public void Create_WithSchemaRef_SetsSchemaRef()
    {
        var id = NewId("Create_Schema");
        var schemaRef = new TemplateSchemaRef(IdGen.Generate("DocumentTemplateAggregateTests:schema-ref"));

        var aggregate = DocumentTemplateAggregate.Create(id, new TemplateName("Form Tmpl"), TemplateType.Form, schemaRef, BaseTime);

        var evt = Assert.IsType<DocumentTemplateCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(schemaRef, evt.SchemaRef);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var t1 = DocumentTemplateAggregate.Create(id, new TemplateName("T"), TemplateType.Generic, null, BaseTime);
        var t2 = DocumentTemplateAggregate.Create(id, new TemplateName("T"), TemplateType.Generic, null, BaseTime);

        Assert.Equal(
            ((DocumentTemplateCreatedEvent)t1.DomainEvents[0]).TemplateId.Value,
            ((DocumentTemplateCreatedEvent)t2.DomainEvents[0]).TemplateId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentTemplateState()
    {
        var id = NewId("History");
        var name = new TemplateName("Historic Template");

        var history = new object[]
        {
            new DocumentTemplateCreatedEvent(id, name, TemplateType.Report, null, BaseTime)
        };

        var aggregate = (DocumentTemplateAggregate)Activator.CreateInstance(typeof(DocumentTemplateAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.TemplateId);
        Assert.Equal(TemplateStatus.Draft, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
