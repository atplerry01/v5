using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed class DocumentTemplateAggregate : AggregateRoot
{
    public DocumentTemplateId TemplateId { get; private set; }
    public TemplateName Name { get; private set; }
    public TemplateType Type { get; private set; }
    public TemplateSchemaRef? SchemaRef { get; private set; }
    public TemplateStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentTemplateAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentTemplateAggregate Create(
        DocumentTemplateId templateId,
        TemplateName name,
        TemplateType type,
        TemplateSchemaRef? schemaRef,
        Timestamp createdAt)
    {
        var aggregate = new DocumentTemplateAggregate();

        aggregate.RaiseDomainEvent(new DocumentTemplateCreatedEvent(
            templateId,
            name,
            type,
            schemaRef,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(
        TemplateName newName,
        TemplateType newType,
        TemplateSchemaRef? newSchemaRef,
        Timestamp updatedAt)
    {
        if (Status == TemplateStatus.Archived)
            throw DocumentTemplateErrors.TemplateArchived();

        if (Name == newName && Type == newType && SchemaRef == newSchemaRef)
            return;

        RaiseDomainEvent(new DocumentTemplateUpdatedEvent(
            TemplateId,
            Name,
            newName,
            Type,
            newType,
            newSchemaRef,
            updatedAt));
    }

    public void Activate(Timestamp activatedAt)
    {
        if (Status == TemplateStatus.Archived)
            throw DocumentTemplateErrors.TemplateArchived();

        if (Status == TemplateStatus.Active)
            throw DocumentTemplateErrors.AlreadyActive();

        if (Status == TemplateStatus.Deprecated)
            throw DocumentTemplateErrors.CannotActivateDeprecated();

        RaiseDomainEvent(new DocumentTemplateActivatedEvent(TemplateId, activatedAt));
    }

    public void Deprecate(string reason, Timestamp deprecatedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw DocumentTemplateErrors.InvalidDeprecationReason();

        if (Status == TemplateStatus.Archived)
            throw DocumentTemplateErrors.TemplateArchived();

        if (Status == TemplateStatus.Deprecated)
            throw DocumentTemplateErrors.AlreadyDeprecated();

        RaiseDomainEvent(new DocumentTemplateDeprecatedEvent(TemplateId, reason.Trim(), deprecatedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == TemplateStatus.Archived)
            throw DocumentTemplateErrors.AlreadyArchived();

        RaiseDomainEvent(new DocumentTemplateArchivedEvent(TemplateId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentTemplateCreatedEvent e:
                TemplateId = e.TemplateId;
                Name = e.Name;
                Type = e.Type;
                SchemaRef = e.SchemaRef;
                Status = TemplateStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case DocumentTemplateUpdatedEvent e:
                Name = e.NewName;
                Type = e.NewType;
                SchemaRef = e.NewSchemaRef;
                LastModifiedAt = e.UpdatedAt;
                break;

            case DocumentTemplateActivatedEvent e:
                Status = TemplateStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case DocumentTemplateDeprecatedEvent e:
                Status = TemplateStatus.Deprecated;
                LastModifiedAt = e.DeprecatedAt;
                break;

            case DocumentTemplateArchivedEvent e:
                Status = TemplateStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }
}
