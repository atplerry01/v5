using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackWindowUpdatedEvent(
    PlaybackId PlaybackId,
    PlaybackWindow PreviousWindow,
    PlaybackWindow NewWindow,
    Timestamp UpdatedAt) : DomainEvent;
