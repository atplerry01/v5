using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Compliance.Audit;

public readonly record struct AuditRecordId
{
    public Guid Value { get; }

    public AuditRecordId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AuditRecordId cannot be empty.");
        Value = value;
    }
}
