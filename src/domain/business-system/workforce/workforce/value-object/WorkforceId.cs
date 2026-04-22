using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Workforce;

public readonly record struct WorkforceId
{
    public Guid Value { get; }

    public WorkforceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "WorkforceId cannot be empty.");
        Value = value;
    }
}
