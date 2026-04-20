using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed record RecordingStartedEvent(
    RecordingId RecordingId,
    StreamRef StreamRef,
    StreamSessionRef? SessionRef,
    Timestamp StartedAt) : DomainEvent;
