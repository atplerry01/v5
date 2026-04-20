using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed record RecordingCompletedEvent(
    RecordingId RecordingId,
    RecordingOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
