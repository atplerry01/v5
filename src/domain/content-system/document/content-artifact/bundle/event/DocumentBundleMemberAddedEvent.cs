using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public sealed record DocumentBundleMemberAddedEvent(
    DocumentBundleId BundleId,
    BundleMemberRef Member,
    Timestamp AddedAt) : DomainEvent;
