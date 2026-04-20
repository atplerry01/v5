using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed record RecordingArchivedEvent(
    RecordingId RecordingId,
    Timestamp ArchivedAt) : DomainEvent;
