using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public sealed record AudioActivatedEvent(
    AudioId AudioId,
    Timestamp ActivatedAt) : DomainEvent;
