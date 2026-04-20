using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleCreatedEvent(
    DocumentBundleId BundleId,
    BundleName Name,
    Timestamp CreatedAt) : DomainEvent;
