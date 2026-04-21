using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

/// Local reference to a content aggregate (DocumentAggregate in content-system).
/// Wraps the canonical shared-kernel `ContentId` so that content identity is
/// strongly typed end-to-end and not mistakable for any other Guid-shaped id.
public readonly record struct DocumentRef
{
    public ContentId Value { get; }

    public DocumentRef(ContentId value)
    {
        Guard.Against(value.Value == Guid.Empty, "DocumentRef cannot reference an empty ContentId.");
        Value = value;
    }
}
