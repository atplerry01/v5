using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public readonly record struct AccountId
{
    public Guid Value { get; }

    public AccountId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AccountId cannot be empty.");
        Value = value;
    }
}
