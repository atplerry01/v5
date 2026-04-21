using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

/// Local reference to a content aggregate (DocumentAggregate in content-system).
/// Wraps the canonical shared-kernel `ContentId` so that content identity is
/// strongly typed end-to-end and not mistakable for any other Guid-shaped id.
public readonly record struct DocumentRef
{
    public ContentId Value { get; }

    public DocumentRef(ContentId value)
    {
        if (value.Value == Guid.Empty)
            throw new ArgumentException("DocumentRef cannot reference an empty ContentId.", nameof(value));

        Value = value;
    }
}
