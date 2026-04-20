using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed record RecordingFailedEvent(
    RecordingId RecordingId,
    RecordingFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
