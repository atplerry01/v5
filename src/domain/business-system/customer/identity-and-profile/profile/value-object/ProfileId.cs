using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public readonly record struct ProfileId
{
    public Guid Value { get; }

    public ProfileId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProfileId cannot be empty.");
        Value = value;
    }
}
