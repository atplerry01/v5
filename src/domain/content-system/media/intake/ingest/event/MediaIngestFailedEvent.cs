using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestFailedEvent(
    MediaIngestId UploadId,
    MediaIngestFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
