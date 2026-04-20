using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed record MediaVersionActivatedEvent(
    MediaVersionId VersionId,
    Timestamp ActivatedAt) : DomainEvent;
