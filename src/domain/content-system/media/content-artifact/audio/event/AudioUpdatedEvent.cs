using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public sealed record AudioUpdatedEvent(
    AudioId AudioId,
    AudioFormat Format,
    AudioDuration Duration,
    ChannelCount Channels,
    Timestamp UpdatedAt) : DomainEvent;
