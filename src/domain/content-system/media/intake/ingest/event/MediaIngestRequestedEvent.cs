using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestRequestedEvent(
    MediaIngestId UploadId,
    MediaIngestSourceRef SourceRef,
    MediaIngestInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
