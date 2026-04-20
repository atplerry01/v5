using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed record DocumentVersionWithdrawnEvent(
    DocumentVersionId VersionId,
    string Reason,
    Timestamp WithdrawnAt) : DomainEvent;
