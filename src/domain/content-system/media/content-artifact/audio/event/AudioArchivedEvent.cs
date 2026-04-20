using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public sealed record AudioArchivedEvent(
    AudioId AudioId,
    Timestamp ArchivedAt) : DomainEvent;
