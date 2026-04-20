using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

/// Reference to the artifact produced by a completed processing job.
public readonly record struct ProcessingOutputRef
{
    public Guid Value { get; }

    public ProcessingOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProcessingOutputRef cannot be empty.");
        Value = value;
    }
}
