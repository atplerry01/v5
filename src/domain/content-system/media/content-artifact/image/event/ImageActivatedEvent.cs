using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public sealed record ImageActivatedEvent(
    ImageId ImageId,
    Timestamp ActivatedAt) : DomainEvent;
