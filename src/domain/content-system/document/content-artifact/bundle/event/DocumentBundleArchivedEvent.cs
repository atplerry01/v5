using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public sealed record DocumentBundleArchivedEvent(
    DocumentBundleId BundleId,
    Timestamp ArchivedAt) : DomainEvent;
