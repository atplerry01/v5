using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestCancelledEvent(
    MediaIngestId UploadId,
    Timestamp CancelledAt) : DomainEvent;
