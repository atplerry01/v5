using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public readonly record struct CustomerId
{
    public Guid Value { get; }

    public CustomerId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CustomerId cannot be empty.");
        Value = value;
    }
}
