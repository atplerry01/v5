using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct ArchiveRef
{
    public Guid Value { get; }

    public ArchiveRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ArchiveRef cannot be empty.");
        Value = value;
    }
}
