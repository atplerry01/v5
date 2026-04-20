using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed record MediaVersionSupersededEvent(
    MediaVersionId VersionId,
    MediaVersionId SuccessorVersionId,
    Timestamp SupersededAt) : DomainEvent;
