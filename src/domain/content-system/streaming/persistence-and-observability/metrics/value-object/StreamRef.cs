using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public readonly record struct StreamRef
{
    public Guid Value { get; }

    public StreamRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StreamRef cannot be empty.");
        Value = value;
    }
}
