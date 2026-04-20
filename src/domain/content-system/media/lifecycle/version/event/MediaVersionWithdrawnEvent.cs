using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Version;

public sealed record MediaVersionWithdrawnEvent(
    MediaVersionId VersionId,
    string Reason,
    Timestamp WithdrawnAt) : DomainEvent;
