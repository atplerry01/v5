namespace Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.File;

public sealed record DocumentFileRegisteredEventSchema(
    Guid AggregateId,
    Guid DocumentId,
    string StorageRef,
    string DeclaredChecksum,
    string MimeType,
    long SizeBytes,
    DateTimeOffset RegisteredAt);

public sealed record DocumentFileIntegrityVerifiedEventSchema(
    Guid AggregateId,
    string VerifiedChecksum,
    DateTimeOffset VerifiedAt);

public sealed record DocumentFileSupersededEventSchema(
    Guid AggregateId,
    Guid SuccessorFileId,
    DateTimeOffset SupersededAt);

public sealed record DocumentFileArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
