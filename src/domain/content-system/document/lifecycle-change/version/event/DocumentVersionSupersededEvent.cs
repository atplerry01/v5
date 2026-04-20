using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

public sealed record DocumentVersionSupersededEvent(
    DocumentVersionId VersionId,
    DocumentVersionId SuccessorVersionId,
    Timestamp SupersededAt) : DomainEvent;
