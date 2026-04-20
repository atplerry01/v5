using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionFailedEvent(
    SessionId SessionId,
    SessionTerminationReason Reason,
    Timestamp FailedAt) : DomainEvent;
