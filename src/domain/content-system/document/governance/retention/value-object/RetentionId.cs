using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public readonly record struct RetentionId
{
    public Guid Value { get; }

    public RetentionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RetentionId cannot be empty.");
        Value = value;
    }
}
