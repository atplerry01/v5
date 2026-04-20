using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed class DocumentAggregate : AggregateRoot
{
    public DocumentId DocumentId { get; private set; }
    public DocumentTitle Title { get; private set; }
    public DocumentType Type { get; private set; }
    public DocumentClassification Classification { get; private set; }
    public DocumentStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentAggregate Create(
        DocumentId documentId,
        DocumentTitle title,
        DocumentType type,
        DocumentClassification classification,
        Timestamp createdAt)
    {
        var aggregate = new DocumentAggregate();

        aggregate.RaiseDomainEvent(new DocumentCreatedEvent(
            documentId,
            title,
            type,
            classification,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void UpdateMetadata(
        DocumentTitle newTitle,
        DocumentType newType,
        DocumentClassification newClassification,
        Timestamp updatedAt)
    {
        if (Status == DocumentStatus.Archived)
            throw DocumentErrors.CannotModifyArchivedDocument();

        if (Title == newTitle && Type == newType && Classification == newClassification)
            return;

        RaiseDomainEvent(new DocumentMetadataUpdatedEvent(
            DocumentId,
            Title,
            newTitle,
            Type,
            newType,
            Classification,
            newClassification,
            updatedAt));
    }

    public void Activate(Timestamp activatedAt)
    {
        if (Status == DocumentStatus.Archived)
            throw DocumentErrors.CannotModifyArchivedDocument();

        if (Status == DocumentStatus.Active)
            throw DocumentErrors.DocumentAlreadyActive();

        RaiseDomainEvent(new DocumentActivatedEvent(DocumentId, activatedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == DocumentStatus.Archived)
            throw DocumentErrors.DocumentAlreadyArchived();

        RaiseDomainEvent(new DocumentArchivedEvent(DocumentId, archivedAt));
    }

    public void Restore(Timestamp restoredAt)
    {
        if (Status != DocumentStatus.Archived)
            throw DocumentErrors.DocumentNotArchived();

        RaiseDomainEvent(new DocumentRestoredEvent(DocumentId, restoredAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentCreatedEvent e:
                DocumentId = e.DocumentId;
                Title = e.Title;
                Type = e.Type;
                Classification = e.Classification;
                Status = DocumentStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case DocumentMetadataUpdatedEvent e:
                Title = e.NewTitle;
                Type = e.NewType;
                Classification = e.NewClassification;
                LastModifiedAt = e.UpdatedAt;
                break;

            case DocumentActivatedEvent e:
                Status = DocumentStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case DocumentArchivedEvent e:
                Status = DocumentStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;

            case DocumentRestoredEvent e:
                Status = DocumentStatus.Active;
                LastModifiedAt = e.RestoredAt;
                break;
        }
    }
}
