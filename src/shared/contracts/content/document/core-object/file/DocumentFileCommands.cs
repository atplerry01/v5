using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.File;

public sealed record RegisterDocumentFileCommand(
    Guid DocumentFileId,
    Guid DocumentId,
    string StorageRef,
    string DeclaredChecksum,
    string MimeType,
    long SizeBytes,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentFileId;
}

public sealed record VerifyDocumentFileIntegrityCommand(
    Guid DocumentFileId,
    string ComputedChecksum,
    DateTimeOffset VerifiedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentFileId;
}

public sealed record SupersedeDocumentFileCommand(
    Guid DocumentFileId,
    Guid SuccessorFileId,
    DateTimeOffset SupersededAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentFileId;
}

public sealed record ArchiveDocumentFileCommand(
    Guid DocumentFileId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => DocumentFileId;
}
