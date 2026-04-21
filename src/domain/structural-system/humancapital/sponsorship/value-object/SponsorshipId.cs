using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Sponsorship;

public readonly record struct SponsorshipId
{
    public Guid Value { get; }

    public SponsorshipId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SponsorshipId cannot be empty.");
        Value = value;
    }
}
