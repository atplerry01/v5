using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed record DocumentVersionSupersededEvent(
    DocumentVersionId VersionId,
    DocumentVersionId SuccessorVersionId,
    Timestamp SupersededAt) : DomainEvent;
