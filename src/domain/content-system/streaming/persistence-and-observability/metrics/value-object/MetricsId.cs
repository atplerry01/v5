using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public readonly record struct MetricsId
{
    public Guid Value { get; }

    public MetricsId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MetricsId cannot be empty.");
        Value = value;
    }
}
