using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public static class ProfileErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProfileId is required and must not be empty.");

    public static DomainException MissingCustomerRef()
        => new DomainInvariantViolationException("Profile must reference a customer.");

    public static DomainException InvalidStateTransition(ProfileStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ProfileId id)
        => new DomainInvariantViolationException($"Profile '{id.Value}' is archived and cannot be mutated.");

    public static DomainException DescriptorNotPresent(string key)
        => new DomainInvariantViolationException($"Profile does not contain descriptor '{key}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Profile has already been initialized.");
}
