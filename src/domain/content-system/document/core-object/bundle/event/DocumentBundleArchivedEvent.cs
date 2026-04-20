using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleArchivedEvent(
    DocumentBundleId BundleId,
    Timestamp ArchivedAt) : DomainEvent;
