using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Processing;

public readonly record struct ProcessingJobId
{
    public Guid Value { get; }

    public ProcessingJobId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProcessingJobId cannot be empty.");
        Value = value;
    }
}
