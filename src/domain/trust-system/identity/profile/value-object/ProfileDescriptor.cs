using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public readonly record struct ProfileDescriptor
{
    public Guid IdentityReference { get; }
    public string DisplayName { get; }
    public string ProfileType { get; }

    public ProfileDescriptor(Guid identityReference, string displayName, string profileType)
    {
        Guard.Against(identityReference == Guid.Empty, "ProfileDescriptor.IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(displayName), "ProfileDescriptor.DisplayName must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(profileType), "ProfileDescriptor.ProfileType must not be empty.");

        IdentityReference = identityReference;
        DisplayName = displayName.Trim();
        ProfileType = profileType.Trim();
    }
}
