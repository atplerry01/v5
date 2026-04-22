using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

/// Reference to a session or replay aggregate id; bare Guid per domain.guard.md rule 13.
public readonly record struct SessionRef
{
    public Guid Value { get; }

    public SessionRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SessionRef cannot be empty.");
        Value = value;
    }
}
