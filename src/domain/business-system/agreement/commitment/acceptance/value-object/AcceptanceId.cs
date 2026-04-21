using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public readonly record struct AcceptanceId
{
    public Guid Value { get; }

    public AcceptanceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AcceptanceId cannot be empty.");
        Value = value;
    }
}
