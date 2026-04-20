using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public sealed record DocumentBundleMemberRemovedEvent(
    DocumentBundleId BundleId,
    BundleMemberRef Member,
    Timestamp RemovedAt) : DomainEvent;
