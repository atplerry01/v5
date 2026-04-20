using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public sealed record ImageArchivedEvent(
    ImageId ImageId,
    Timestamp ArchivedAt) : DomainEvent;
