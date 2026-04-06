namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public static class AccessProfileErrors
{
    public static DomainException NotFound(Guid profileId)
        => new("ACCESS_PROFILE_NOT_FOUND", $"Access profile '{profileId}' was not found.");

    public static DomainException Suspended(Guid profileId)
        => new("ACCESS_PROFILE_SUSPENDED", $"Access profile '{profileId}' is suspended.");
}
