using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackEnabledEvent(
    PlaybackId PlaybackId,
    Timestamp EnabledAt) : DomainEvent;
