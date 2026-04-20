using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestCompletedEvent(
    MediaIngestId UploadId,
    MediaIngestOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
