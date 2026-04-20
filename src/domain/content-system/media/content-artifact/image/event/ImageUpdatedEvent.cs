using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public sealed record ImageUpdatedEvent(
    ImageId ImageId,
    ImageDimensions Dimensions,
    ImageOrientation Orientation,
    Timestamp UpdatedAt) : DomainEvent;
