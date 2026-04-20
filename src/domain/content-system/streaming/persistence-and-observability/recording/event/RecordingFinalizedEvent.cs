using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed record RecordingFinalizedEvent(
    RecordingId RecordingId,
    Timestamp FinalizedAt) : DomainEvent;
