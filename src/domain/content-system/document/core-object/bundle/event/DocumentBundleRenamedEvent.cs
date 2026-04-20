using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleRenamedEvent(
    DocumentBundleId BundleId,
    BundleName PreviousName,
    BundleName NewName,
    Timestamp RenamedAt) : DomainEvent;
