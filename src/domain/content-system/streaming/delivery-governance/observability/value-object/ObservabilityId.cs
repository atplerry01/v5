using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct ObservabilityId
{
    public Guid Value { get; }

    public ObservabilityId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ObservabilityId cannot be empty.");
        Value = value;
    }
}
