using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public static class BindingErrors
{
    public static DomainException BindingAlreadyReleased()
        => new("Binding has already been released.");

    public static DomainException BindingAlreadyTransferred()
        => new("Binding has already been transferred.");

    public static DomainException BindingNotActive()
        => new("Binding is not in Active status.");

    public static DomainException InvalidOwnerId()
        => new("Owner ID cannot be empty.");

    public static DomainException InvalidAccountId()
        => new("Account ID cannot be empty.");

    public static DomainException CannotTransferReleasedBinding()
        => new("Cannot transfer a binding that has been released.");

    public static DomainException CannotReleaseTransferredBinding()
        => new("Cannot release a binding that has been transferred.");

    public static DomainException OwnerMismatch(Guid expected, Guid actual)
        => new($"Owner mismatch: expected '{expected}', actual '{actual}'.");
}
