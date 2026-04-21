using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public readonly record struct UsageRecordId
{
    public Guid Value { get; }

    public UsageRecordId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "UsageRecordId cannot be empty.");
        Value = value;
    }
}
