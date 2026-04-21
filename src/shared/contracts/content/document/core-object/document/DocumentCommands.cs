using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;

public sealed record CreateDocumentCommand(
    Guid DocumentId,
    string Title,
    string Type,
    string Classification,
    Guid StructuralOwnerId,
    string BusinessOwnerKind,
    Guid BusinessOwnerId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}

public sealed record UpdateDocumentMetadataCommand(
    Guid DocumentId,
    string NewTitle,
    string NewType,
    string NewClassification,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}

public sealed record AttachDocumentVersionCommand(
    Guid DocumentId,
    Guid VersionId,
    DateTimeOffset AttachedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}

public sealed record ActivateDocumentCommand(
    Guid DocumentId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}

public sealed record ArchiveDocumentCommand(
    Guid DocumentId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}

public sealed record RestoreDocumentCommand(
    Guid DocumentId,
    DateTimeOffset RestoredAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}

public sealed record SupersedeDocumentCommand(
    Guid DocumentId,
    Guid SupersedingDocumentId,
    DateTimeOffset SupersededAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentId;
}
