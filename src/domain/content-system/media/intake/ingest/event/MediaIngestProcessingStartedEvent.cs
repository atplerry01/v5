using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestProcessingStartedEvent(
    MediaIngestId UploadId,
    Timestamp StartedAt) : DomainEvent;
