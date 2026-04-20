namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public static class ProfileErrors
{
    public static ProfileDomainException MissingId()
        => new("ProfileId is required and must not be empty.");

    public static ProfileDomainException MissingCustomerRef()
        => new("Profile must reference a customer.");

    public static ProfileDomainException InvalidStateTransition(ProfileStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProfileDomainException ArchivedImmutable(ProfileId id)
        => new($"Profile '{id.Value}' is archived and cannot be mutated.");

    public static ProfileDomainException DescriptorNotPresent(string key)
        => new($"Profile does not contain descriptor '{key}'.");
}

public sealed class ProfileDomainException : Exception
{
    public ProfileDomainException(string message) : base(message) { }
}
