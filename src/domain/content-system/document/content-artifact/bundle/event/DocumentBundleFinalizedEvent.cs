using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public sealed record DocumentBundleFinalizedEvent(
    DocumentBundleId BundleId,
    Timestamp FinalizedAt) : DomainEvent;
