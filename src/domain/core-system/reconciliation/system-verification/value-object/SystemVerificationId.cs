using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

public readonly record struct SystemVerificationId
{
    public Guid Value { get; }

    public SystemVerificationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SystemVerificationId cannot be empty.");
        Value = value;
    }
}
