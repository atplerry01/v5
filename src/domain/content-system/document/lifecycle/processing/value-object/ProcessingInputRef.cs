using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Processing;

/// Reference to the input document/version/artifact a processing job operates on.
/// Carried as a bare id to avoid cross-BC type imports per domain.guard.md rule 13.
public readonly record struct ProcessingInputRef
{
    public Guid Value { get; }

    public ProcessingInputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProcessingInputRef cannot be empty.");
        Value = value;
    }
}
