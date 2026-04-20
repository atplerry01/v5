using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleFinalizedEvent(
    DocumentBundleId BundleId,
    Timestamp FinalizedAt) : DomainEvent;
