using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public readonly record struct StreamAccessId
{
    public Guid Value { get; }

    public StreamAccessId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StreamAccessId cannot be empty.");
        Value = value;
    }
}
